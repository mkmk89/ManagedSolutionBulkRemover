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
                logger.Log($"Deleting solutions: {string.Join(",", solutionsNames)}", Color.Black);
                worker.ReportProgress(-1, "Collecting solutions dependencies...");
                List<Solution> solutionsForDelete = new List<Solution>();
                List<Tuple<Guid, string>> activeLayerToBeRemoved = new List<Tuple<Guid, string>>();
                CollectForDeletion(Service, solutionsNames, solutionsForDelete, activeLayerToBeRemoved, logger);

                if (deleteActiveLayers)
                {
                    worker.ReportProgress(-1, "Deleting Active (unamanaged layers)...");

                    foreach (var layer in activeLayerToBeRemoved)
                    {
                        DeleteLayer(Service, layer.Item1, layer.Item2, logger);
                    }
                    //reiterate after deleting Active layers
                    solutionsForDelete = new List<Solution>();
                    activeLayerToBeRemoved = new List<Tuple<Guid, string>>();
                    CollectForDeletion(Service, solutionsNames, solutionsForDelete, activeLayerToBeRemoved, logger);
                }
                else
                {
                    foreach (var layer in activeLayerToBeRemoved)
                        logger.Log($"There is an active layer for component {layer.Item2} id: {layer.Item1} but wasn't deleted", Color.Orange);
                }
                worker.ReportProgress(-1, "Checking if any dependencies are unselected...");

                List<string> missingSelection = new List<string>();
                foreach (var solution in solutionsForDelete)
                    CheckIfMissing(worker, Service, solution, solutionsNames, missingSelection, logger);

                if (missingSelection.Count > 0)
                {
                    foreach (var solutionName in missingSelection.Distinct())
                        logger.Log($"Solution {solutionName} is required for deletion selected soltuins", Color.Red);
                }
                else
                {
                    foreach (var solution in solutionsForDelete)
                        DeletePass(worker, Service, solution, logger);
                    logger.Log($"Deleted all solutions listed", Color.LightGray);
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

        void CollectForDeletion(IOrganizationService client, List<string> solutionsNames, List<Solution> sols, List<Tuple<Guid, string>> activeLayersToBeDeleted, Logger logger)
        {
            foreach (var solutionName in solutionsNames)
            {
                if (solutionName == "System" || solutionName == "Active")
                    continue;
                var solution = CheckIfExists(client, solutionName);
                if (solution != null)
                {
                    try
                    {
                        Solution sol;
                        if (sols.Any(x => x.UniqueName.Equals(solutionName)))
                            sol = sols.Single(x => x.UniqueName.Equals(solutionName));
                        else
                        {
                            sol = new Solution(solution.Id, solution.GetAttributeValue<string>("uniquename"), solution.GetAttributeValue<EntityReference>("parentsolutionid")) { Entity = solution };
                            sols.Add(sol);
                        }

                        var dependentComponents = ((RetrieveDependenciesForUninstallResponse)
                            client.Execute(new RetrieveDependenciesForUninstallRequest
                            {
                                SolutionUniqueName = sol.UniqueName
                            })).EntityCollection.Entities.ToList();

                        if (dependentComponents.Count > 0)
                        {

                            var dependantSolutions = new List<Entity>();

                            foreach (Entity dependence in dependentComponents)
                            {
                                Guid dependentcomponentobjectid = dependence.GetAttributeValue<Guid>("dependentcomponentobjectid");
                                OptionSetValue dependentcomponenttype = dependence.GetAttributeValue<OptionSetValue>("dependentcomponenttype");
                                string solutionComponentName = getComponentTypeName(dependentcomponenttype.Value);
                                // QueryLayers
                                var query = new QueryExpression("msdyn_componentlayer");
                                query.ColumnSet.AllColumns = true;
                                query.AddOrder("msdyn_order", OrderType.Ascending);
                                query.Criteria.AddCondition("msdyn_componentid", ConditionOperator.Equal, dependentcomponentobjectid);
                                query.Criteria.AddCondition("msdyn_solutioncomponentname", ConditionOperator.Equal, solutionComponentName);

                                var layers = client.RetrieveMultiple(query).Entities.ToList();
                                foreach (var layer in layers)
                                {
                                    string layerSolName = layer["msdyn_solutionname"] as string;
                                    if (layerSolName == "Active")
                                    {
                                        activeLayersToBeDeleted.Add(new Tuple<Guid, string>(dependentcomponentobjectid, solutionComponentName));
                                        continue;
                                    }
                                    if (layerSolName != solutionName)
                                    {
                                        dependantSolutions.AddRange(client.RetrieveMultiple(new QueryExpression("solution")
                                        {
                                            ColumnSet = new ColumnSet(true),
                                            Criteria = new FilterExpression
                                            {
                                                Conditions =
                                                    {
                                                        new ConditionExpression("uniquename", ConditionOperator.Equal, layerSolName)
                                                    }
                                            },

                                        }).Entities);
                                    }
                                    else
                                        break;//if reached own layer don't consider those below
                                }
                            }

                            foreach (var ds in dependantSolutions.Distinct())
                            {
                                if (ds.GetAttributeValue<string>("uniquename").Equals("Active") || ds.GetAttributeValue<string>("uniquename").Equals("System"))
                                {
                                    continue;
                                }
                                if (ds.Id.Equals(sol.Id))
                                    continue;
                                var existingSolution = sols.FirstOrDefault(s => s.Id == ds.Id);
                                if (existingSolution == null)
                                {
                                    existingSolution = new Solution(ds.Id, ds.GetAttributeValue<string>("uniquename"), ds.GetAttributeValue<EntityReference>("parentsolutionid"));
                                    existingSolution.Entity = ds;
                                    sols.Add(existingSolution);
                                }
                                existingSolution.RequiredSolutions.Add(sol);
                                sol.DependentSolutions.Add(existingSolution);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(ex.Message, Color.Red);
                    }
                }
            }
        }

        private void DeleteLayer(IOrganizationService client, Guid componentId, string componentName, Logger logger)
        {
            var req = new OrganizationRequest("RemoveActiveCustomizations");
            req.Parameters.Add("ComponentId", componentId);
            req.Parameters.Add("SolutionComponentName", componentName);
            try
            {
                Service.Execute(req);
                logger.Log($"Deleted component {componentId} type {componentName} Active (unmanaged) layer", Color.Green);
            }
            catch (Exception ex)
            {
                logger.Log($"Error at solution layer deletion: {ex.Message}", Color.Red);
            }
        }

        void DeletePass(BackgroundWorker worker, IOrganizationService client, Solution solution, Logger logger)
        {
            foreach (Solution dependentSolution in solution.DependentSolutions)
            {
                DeletePass(worker, client, dependentSolution, logger);
            }
            if (CheckIfExists(client, solution.UniqueName) != null)
            {
                worker.ReportProgress(-1, $"Deleting solution {solution.UniqueName}...");
                Delete(client, solution, logger);
            }
        }

        void CheckIfMissing(BackgroundWorker worker, IOrganizationService client, Solution solution, List<string> solutionsNames, List<string> missingSelection, Logger logger)
        {
            foreach (Solution dependentSolution in solution.DependentSolutions.ToList())
            {
                if (!solutionsNames.Contains(dependentSolution.UniqueName))
                {
                    missingSelection.Add($"{solution.UniqueName} depends on {dependentSolution.UniqueName} solution, which is not selected for deletion");
                }
                else
                {
                    solution.DependentSolutions.Remove(dependentSolution);
                }
                CheckIfMissing(worker, client, dependentSolution, solutionsNames, missingSelection, logger);
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

        public string getComponentTypeName(int componentType)
        {
            switch (componentType)
            {
                case 1:
                    return
                    "Entity";
                case 2:
                    return
                   "Attribute";
                case 3:
                    return
                   "Relationship";
                case 4:
                    return
                   "AttributePicklistValue";
                case 5:
                    return
                   "AttributeLookupValue";
                case 6:
                    return
                   "ViewAttribute";
                case 7:
                    return
                   "LocalizedLabel";
                case 8:
                    return
                   "RelationshipExtraCondition";
                case 9:
                    return
                   "OptionSet";
                case 10:
                    return
                   "EntityRelationship";
                case 11:
                    return
                   "EntityRelationshipRole";
                case 12:
                    return
                   "EntityRelationshipRelationships";
                case 13:
                    return
                   "ManagedProperty";
                case 14:
                    return
                   "EntityKey";
                case 16:
                    return
                   "Privilege";
                case 17:
                    return
                   "PrivilegeObjectTypeCode";
                case 20:
                    return
                   "Role";
                case 21:
                    return
                   "RolePrivilege";
                case 22:
                    return
                   "DisplayString";
                case 23:
                    return
                   "DisplayStringMap";
                case 24:
                    return
                   "Form";
                case 25:
                    return
                   "Organization";
                case 26:
                    return
                   "SavedQuery";
                case 29:
                    return
                   "Workflow";
                case 31:
                    return
                   "Report";
                case 32:
                    return
                   "ReportEntity";
                case 33:
                    return
                   "ReportCategory";
                case 34:
                    return
                   "ReportVisibility";
                case 35:
                    return
                   "Attachment";
                case 36:
                    return
                   "EmailTemplate";
                case 37:
                    return
                   "ContractTemplate";
                case 38:
                    return
                   "KBArticleTemplate";
                case 39:
                    return
                   "MailMergeTemplate";
                case 44:
                    return
                   "DuplicateRule";
                case 45:
                    return
                   "DuplicateRuleCondition";
                case 46:
                    return
                   "EntityMap";
                case 47:
                    return
                   "AttributeMap";
                case 48:
                    return
                   "RibbonCommand";
                case 49:
                    return
                   "RibbonContextGroup";
                case 50:
                    return
                   "RibbonCustomization";
                case 52:
                    return
                   "RibbonRule";
                case 53:
                    return
                   "RibbonTabToCommandMap";
                case 55:
                    return
                   "RibbonDiff";
                case 59:
                    return
                   "SavedQueryVisualization";
                case 60:
                    return
                   "SystemForm";
                case 61:
                    return
                   "WebResource";
                case 62:
                    return
                   "SiteMap";
                case 63:
                    return
                   "ConnectionRole";
                case 64:
                    return
                   "ComplexControl";
                case 70:
                    return
                   "FieldSecurityProfile";
                case 71:
                    return
                   "FieldPermission";
                case 80:
                    return
                    "AppModule";
                case 90:
                    return
                   "PluginType";
                case 91:
                    return
                   "PluginAssembly";
                case 92:
                    return
                   "SDKMessageProcessingStep";
                case 93:
                    return
                   "SDKMessageProcessingStepImage";
                case 95:
                    return
                   "ServiceEndpoint";
                case 150:
                    return
                   "RoutingRule";
                case 151:
                    return
                   "RoutingRuleItem";
                case 152:
                    return
                   "SLA";
                case 153:
                    return
                   "SLAItem";
                case 154:
                    return
                   "ConvertRule";
                case 155:
                    return
                   "ConvertRuleItem";
                case 65:
                    return
                   "HierarchyRule";
                case 161:
                    return
                   "MobileOfflineProfile";
                case 162:
                    return
                   "MobileOfflineProfileItem";
                case 165:
                    return
                   "SimilarityRule";
                case 66:
                    return
                   "CustomControl";
                case 68:
                    return
                   "CustomControlDefaultConfig";
                case 166:
                    return
                   "DataSourceMapping";
                case 201:
                    return
                   "SDKMessage";
                case 202:
                    return
                   "SDKMessageFilter";
                case 203:
                    return
                   "SdkMessagePair";
                case 204:
                    return
                   "SdkMessageRequest";
                case 205:
                    return
                   "SdkMessageRequestField";
                case 206:
                    return
                   "SdkMessageResponse";
                case 207:
                    return
                   "SdkMessageResponseField";
                case 210:
                    return
                   "WebWizard";
                case 18:
                    return
                   "Index";
                case 208:
                    return
                   "ImportMap";
                case 300:
                    return
                   "CanvasApp";
                case 371:
                    return
                   "Connector";
                case 372:
                    return
                   "Connector";
                case 380:
                    return
                   "EnvironmentVariableDefinition";
                case 381:
                    return
                   "EnvironmentVariableValue";
                case 400:
                    return
                   "AIProjectType";
                case 401:
                    return
                   "AIProject";
                case 402:
                    return
                   "AIConfiguration";
                case 430:
                    return
                   "EntityAnalyticsConfiguration";
                case 431:
                    return
                   "AttributeImageConfiguration";
                case 432:
                    return
                   "EntityImageConfiguration";
                default: throw new Exception("Unsupported component type");
            }
        }
    }
}
