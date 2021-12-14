using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using System.ComponentModel;
using System.Drawing;
using XrmToolBox.Extensibility.Args;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using System.Windows.Controls;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Net;
using System.Web.Services.Protocols;

namespace ManagedSolutionBulkRemover
{
    public class Logic
    {

        IOrganizationService Service;
        public Logic(IOrganizationService service)
        {
            this.Service = service;
        }

        public EntityCollection GetSolutions()
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("uniquename", "friendlyname", "ismanaged", "version")
            };

            var filter = new FilterExpression();
            filter.AddCondition("ismanaged", ConditionOperator.Equal, true);
            filter.AddCondition("isvisible", ConditionOperator.Equal, true);
            query.Criteria.AddFilter(filter);
            return Service.RetrieveMultiple(query);
        }

        internal void RemoveSolutions(BackgroundWorker worker, List<string> solutionsNames, Logger logger)
        {
            try
            {
                logger.Log($"Deleting solutions: {Environment.NewLine} {string.Join(Environment.NewLine, solutionsNames)}", Color.LightGray);

                List<Solution> solutionsFailed = new List<Solution>();

                do
                {
                    List<Solution> solutionsForDelete = new List<Solution>();
                    worker.ReportProgress(-1, "Collecting solutions dependencies...");
                    CollectForDeletion(Service, solutionsNames, solutionsForDelete, logger);
                    foreach (var solution in solutionsForDelete.ToList())
                    {
                        while (IfAnySolutionJobsRunning(Service))
                        {
                            worker.ReportProgress(-1, $"Waiting for running solution jobs to finish...");
                            Thread.Sleep(TimeSpan.FromSeconds(30));
                        }
                        worker.ReportProgress(-1, $"Deleting solution {solution.UniqueName}...");
                        Delete(Service, solution, solutionsFailed, logger);
                        solutionsForDelete.Remove(solution);
                    }
                }
                while (solutionsNames.Count > 0);

                if (solutionsFailed.Count == 0)
                    logger.Log($"Deleted all solutions listed", Color.LightGray);
                else
                {
                    logger.Log($"All solutions were processed, but {solutionsFailed.Count} couldn't be deleted. Check dependencies in Dynamics.", Color.LightGray);
                    foreach (var solution in solutionsFailed)
                        logger.Log($"Solution {solution.UniqueName} can't be deleted, check dependencies", Color.Red);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error occured: {ex.Message}", Color.Red);

            }
        }

        Entity CheckIfExists(IOrganizationService client, string solutionName)
        {
            QueryExpression queryImportedSolution = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "solutionid", "friendlyname", "uniquename", "parentsolutionid" }),
                Criteria = new FilterExpression()
            };

            queryImportedSolution.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionName);

            var ImportedSolution = client.RetrieveMultiple(queryImportedSolution).Entities;
            if (ImportedSolution.Count > 0)
                return ImportedSolution[0];
            else
                return null;
        }

        void CollectForDeletion(IOrganizationService client, List<string> solutionsNames, List<Solution> solutionsToDelete, Logger logger)
        {
            foreach (var solutionName in solutionsNames.ToList())
            {
                if (solutionName == "System" || solutionName == "Active")
                    continue;
                var solution = CheckIfExists(client, solutionName);
                if (solution != null)
                {
                    try
                    {
                        Solution sol = new Solution(solution.Id, solution.GetAttributeValue<string>("uniquename"), solution.GetAttributeValue<EntityReference>("parentsolutionid")) { Entity = solution };
                        var dependentComponents = ((RetrieveDependenciesForUninstallResponse)
                            client.Execute(new RetrieveDependenciesForUninstallRequest
                            {
                                SolutionUniqueName = sol.UniqueName
                            })).EntityCollection.Entities.ToList();

                        if (dependentComponents.Count == 0)
                        {
                            solutionsToDelete.Add(sol);
                            solutionsNames.Remove(solutionName);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Getting dependencies failed. Those will be reprocessed. Error: {ex.Message}", Color.Red);
                    }
                }
                else
                    solutionsNames.Remove(solutionName);
            }
        }

        void Delete(IOrganizationService client, Solution solution, List<Solution> solutionsFailed, Logger logger)
        {
            string solutionName = solution.UniqueName;
            logger.Log($"Attempt to delete solution: {solutionName}", Color.LightGray);
            try
            {
                client.Delete("solution", solution.Id);
            }
            catch (Exception ex)
            {
                if (ex is Exception || ex is System.ServiceModel.CommunicationException)
                {
                    try
                    {
                        while (IsUninstallRunning(client, solutionName))
                            Thread.Sleep(TimeSpan.FromSeconds(30));
                    }
                    catch (Exception ex2)
                    {
                        logger.Log($"Error at solution {solution.UniqueName} deletion: {ex2.Message}", Color.Red);
                        solutionsFailed.Add(solution);
                        return;
                    }
                }
                else
                {
                    logger.Log($"Error at solution {solution.UniqueName} deletion: {ex.Message}", Color.Red);
                    solutionsFailed.Add(solution);
                    return;
                }
            }
            logger.Log($"Deleted solution {solution.UniqueName}", Color.Green);
        }

        bool IsUninstallRunning(IOrganizationService client, string solutionName)
        {
            string fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='msdyn_solutionhistory'>
                        <attribute name='msdyn_status' />
                        <attribute name='msdyn_operation' />
                        <attribute name='msdyn_exceptionmessage' />
                        <attribute name='msdyn_result' />
                        <attribute name='msdyn_solutionhistoryid' />
                        <filter type='and'>
                          <condition attribute='msdyn_operation' operator='eq' value='1' />
                          <condition attribute='msdyn_name' operator='eq' value='{solutionName}' />
                        </filter>
                      </entity>
                    </fetch>";
            var query = new FetchExpression(fetch);
            var result = client.RetrieveMultiple(query).Entities;
            if (result.Where(x => ((OptionSetValue)x["msdyn_status"]).Value == 0).ToList().Count > 0)//there is running job
                return true;
            else if (result.Where(x => ((OptionSetValue)x["msdyn_status"]).Value == 1 && ((bool)x["msdyn_result"]) == false).ToList().Count > 0)//completed but failed
                throw new Exception(result.Where(x => ((bool)x["msdyn_result"]) == false).First().GetAttributeValue<string>("msdyn_exceptionmessage"));
            else
                return false;
        }

        bool IfAnySolutionJobsRunning(IOrganizationService client)
        {
            string fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='msdyn_solutionhistory'>
                        <attribute name='msdyn_name' />
                        <attribute name='msdyn_suboperation' />
                        <attribute name='msdyn_status' />
                        <attribute name='msdyn_solutionversion' />
                        <attribute name='msdyn_operation' />
                        <attribute name='msdyn_publishername' />
                        <attribute name='msdyn_starttime' />
                        <attribute name='msdyn_errorcode' />
                        <attribute name='msdyn_endtime' />
                        <attribute name='msdyn_result' />
                        <attribute name='msdyn_solutionhistoryid' />
                        <filter type='and'>
                          <condition attribute='msdyn_status' operator='eq' value='0' />
                        </filter>
                      </entity>
                    </fetch>";
            var query = new FetchExpression(fetch);
            if (client.RetrieveMultiple(query).Entities.Count == 0)
                return false;
            else
                return true;
        }
    }
}
