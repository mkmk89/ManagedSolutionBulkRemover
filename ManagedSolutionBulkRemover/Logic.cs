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

        internal void RemoveSolutions(BackgroundWorker worker, List<string> solutionsNames, bool deleteActiveLayers, Logger logger)
        {
            try
            {
                logger.Log($"Deleting solutions: {Environment.NewLine} {string.Join(Environment.NewLine, solutionsNames)}", Color.LightGray);
                worker.ReportProgress(-1, "Collecting solutions dependencies...");

                List<Solution> solutionsForDelete = new List<Solution>();

                do
                {
                    CollectForDeletion(Service, solutionsNames, solutionsForDelete, deleteActiveLayers, logger);
                    foreach (var solution in solutionsForDelete.ToList())
                    {
                        Delete(Service, solution, logger);
                        solutionsForDelete.Remove(solution);
                    }
                }
                while (solutionsForDelete.Count == 0);

                if (solutionsNames.Count == 0)
                    logger.Log($"Deleted all solutions listed", Color.LightGray);
                else
                {
                    foreach (var solutionName in solutionsNames)
                        logger.Log($"Solution {solutionName} can't be deleted due to existing dependencies", Color.Red);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error occured: {ex.Message}{Environment.NewLine}", Color.Red);

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

        void CollectForDeletion(IOrganizationService client, List<string> solutionsNames, List<Solution> solutionsToDelete, bool deleteActiveLayers, Logger logger)
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
                        logger.Log(ex.Message, Color.Red);
                    }
                }
            }
        }

        void Delete(IOrganizationService client, Solution solution, Logger logger)
        {
            string solutionName = solution.UniqueName;
            logger.Log($"Attempt to delete solution: {solutionName}", Color.LightGray);
            try
            {
                client.Delete("solution", solution.Id);
            }
            catch (System.TimeoutException ex)
            {
                while (CheckIfExists(client, solutionName) != null)
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
                          <condition attribute='msdyn_operation' operator='eq' value='1' />
                          <condition attribute='msdyn_status' operator='eq' value='0' />
                          <condition attribute='msdyn_name' operator='eq' value='{solutionName}' />
                        </filter>
                      </entity>
                    </fetch>";
                    var query = new FetchExpression(fetch);
                    if (client.RetrieveMultiple(query).Entities.Count == 0)
                        break;
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error at solution deletion: {ex.Message}", Color.Red);
            }
            logger.Log($"Deleted solution {solution.FriendlyName}", Color.Green);
        }
    }
}
