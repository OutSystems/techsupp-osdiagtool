using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OSDiagTool.DBConnector;

namespace OSDiagTool.Platform
{
    public class PlatformDBIntegrity
    {
        /* To add more metamodel checks:
         * 1. create a private static string with the check name;
         * 2. add the static string check key, sql text, error message, if it is expected the sql to return records or not and checkOk=null in the integrityModel;
         * 3. upddate the GetUpdatedErrorMessages() method to return the information with the details of the metamodel related to the integrity issue if necessary, otherwise, it returns default and prints only the error message from point 2
         * Assumptions:
         * - the sql should validate the integrity by returning or not records - that needs to be detailed in the integrity model in returnsRecords bool --> check RunDatabaseCheck() method return
        */
        private static string _appSettingsConfig = "appSettings.config";
        private static string _runtimeConnectionStringKey = "OutSystems.DB.Platform.Application.Runtime.ConnectionString (DEFAULT)";
        private static string _sessionConnectionStringKey = "OutSystems.DB.Platform.Application.Session.ConnectionString (DEFAULT)";
        private static string _loggingConnectingStringKey = "OutSystems.DB.Logging.Application.Runtime.ConnectionString (DEFAULT)";
        private static string _genericErrorMessage = "== A Platform integrity issue was found - please open a support case - ";
        //Check strings
        private static string rpm_4012_espace_check = "RPM-4012-eSpace - NOT OK";
        private static string rpm_4012_extension_check = "RPM-4012-extension - NOT OK";
        private static string devs_Tenants_check = "Developers_Tenants - NOT OK";
        private static string moduleConnectionStrings_check = "ModuleConnectionStrings - NOT OK";
        private static string modulesLinkedToApp_check = "ModulesLinkedToApp - NOT OK";
        private static string duplicateUsernames_check = "DuplicateUsernames - NOT OK";
        private static string sessionModelVersion_check = "SessionModelVersion - NOT OK";
        private static string clientApplicationTokenExists_check = "ClientApplicationTokenExists - NOT OK";
        private static string metamodelVersion_check = "MetamodelVersion - NOT OK";
        private static string systemComponentsInstalled_check = "SystemComponentsInstalled - NOT OK";

        private static List<List<object>> sqlResult = new List<List<object>>();
        public static Integrity.IntegrityModel integrityModel = new Integrity.IntegrityModel();
        private static Dictionary<string, Dictionary<string, DateTime>> allRunningFolders = new Dictionary<string, Dictionary<string, DateTime>>();

        public static Dictionary<string, bool?> ServerChecks = new Dictionary<string, bool?>()
        {
            { moduleConnectionStrings_check, null },
            { systemComponentsInstalled_check, null }
        };

        public static void InitializeIntegrityModel()
        {
            string platformRunningPath = Path.Combine(Program._osInstallationFolder, "running");

            integrityModel.CheckDetails = new Dictionary<string, Integrity.IntegrityDetails>
            {
                { rpm_4012_espace_check, new Integrity.IntegrityDetails { SqlText = @"SELECT E.ID, M.ESPACE_ID
                                    FROM OSSYS_ESPACE E
                                    LEFT JOIN OSSYS_MODULE M ON E.ID = M.ESPACE_ID
                                    WHERE M.ESPACE_ID IS NULL AND E.IS_ACTIVE = 1", ErrorMessage = _genericErrorMessage +"provide the output of OSDiagTool and the contents of the tables OSSYS_ESPACE and OSSYS_MODULE ==", returnsRecords = false , checkOk = null }  },
                { rpm_4012_extension_check, new Integrity.IntegrityDetails { SqlText = @"SELECT E.ID, M.EXTENSION_ID
                                    FROM OSSYS_EXTENSION E
                                    LEFT JOIN OSSYS_MODULE M ON E.ID = M.EXTENSION_ID
                                    WHERE M.EXTENSION_ID IS NULL AND E.IS_ACTIVE = 1", ErrorMessage = _genericErrorMessage +"provide the output of OSDiagTool and the contents of the tables OSSYS_EXTENSION and OSSYS_MODULE ==", returnsRecords = false , checkOk = null }  },
                { devs_Tenants_check, new Integrity.IntegrityDetails { SqlText = @"SELECT DISTINCT UD.USER_ID, US.TENANT_ID FROM OSSYS_USER_DEVELOPER UD
                                    LEFT JOIN OSSYS_USER US ON UD.USER_ID=US.ID
                                    WHERE US.IS_ACTIVE=1 AND US.TENANT_ID <> 1", ErrorMessage = _genericErrorMessage +"Developers with wront tenants (expected tenant ID 1) were found - provide the output of OSDiagTool ==", returnsRecords = false , checkOk = null }  },
                { modulesLinkedToApp_check, new Integrity.IntegrityDetails { SqlText = @"SELECT M.ID, M.ESPACE_ID, M.EXTENSION_ID
                                    FROM OSSYS_MODULE M
                                    LEFT JOIN OSSYS_APP_DEFINITION_MODULE APPDEF ON M.ID = APPDEF.MODULE_ID
                                    LEFT JOIN OSSYS_ESPACE E ON M.ESPACE_ID = E.ID
                                    LEFT JOIN OSSYS_EXTENSION EXT ON M.EXTENSION_ID = EXT.ID
                                    WHERE APPDEF.MODULE_ID IS NULL
                                    AND (E.IS_ACTIVE = 1 OR EXT.IS_ACTIVE = 1)", ErrorMessage = _genericErrorMessage +"provide the contents of the tables OSSYS_ESPACE, OSSYS_EXTENSION, OSSYS_MODULE, OSSYS_APP_DEFINITION_MODULE and the output of OSDiagTool ==", returnsRecords = false , checkOk = null }  },
                { duplicateUsernames_check, new Integrity.IntegrityDetails { SqlText = @"SELECT USERNAME, TENANT_ID, COUNT(1)
                                    FROM OSSYS_USER 
                                    WHERE IS_ACTIVE = 1
                                    GROUP BY TENANT_ID, USERNAME
                                    HAVING COUNT(1) > 1", ErrorMessage = _genericErrorMessage +"Duplicate usernames in the same tenant were found - please review your User Management application and review the duplicate users found in OSSYS_USER ==", returnsRecords = false , checkOk = null }  },
                { sessionModelVersion_check, new Integrity.IntegrityDetails { SqlText = String.Format(@"SELECT VAL FROM OSSYS_PARAMETER 
                                    WHERE LOWER(NAME) = 'sessionversion'
                                    AND VAL = '{0}'", Program.osPlatformVersion), ErrorMessage = _genericErrorMessage +"Session Model version found is different from Platform version " + Program.osPlatformVersion + " - upgrade the session model by recreating the session database in Configuration Tool ==", returnsRecords = true , checkOk = null }  },
                { clientApplicationTokenExists_check, new Integrity.IntegrityDetails { SqlText = @"SELECT VAL FROM OSSYS_PARAMETER 
                                    WHERE LOWER(NAME) = 'clientapplicationtoken'
                                    AND VAL IS NOT NULL", ErrorMessage = _genericErrorMessage +"ClientApplicationToken is missing - please provide the contents of the OSSYS_PARAMETER table ==", returnsRecords = true , checkOk = null }  },
                { metamodelVersion_check, new Integrity.IntegrityDetails { SqlText = String.Format(@"SELECT VAL FROM OSSYS_PARAMETER 
                                    WHERE LOWER(NAME) = 'version' AND VAL = '{0}'", Program.osPlatformVersion), ErrorMessage = _genericErrorMessage +" - Metamodel version is different from Platform version " + Program.osPlatformVersion + " - please provide the contents of the OSSYS_PARAMETER table ==", returnsRecords = true , checkOk = null }  },
            };

            allRunningFolders = Integrity.IntegrityHelper.GetLastModulesRunningPublished(platformRunningPath); // <path folder>, [<module name>, <creation date>] - check for duplicates and use latest created only            
        }

        public static void RunIntegrityCheck(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination,  DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string oracleAdminSchema = null)
        {
            InitializeIntegrityModel();
            IDatabaseConnection connection = DatabaseConnectionFactory.GetDatabaseConnection(dbEngine, SQLConnectionString, OracleConnectionString);
            IDatabaseCommand commandExecutor = DatabaseCommandFactory.GetCommandExecutor(dbEngine, connection);

            using (connection)
            {
                Integrity.IntegrityModel integrityModelCopy = new Integrity.IntegrityModel(integrityModel);
                foreach (string key in integrityModelCopy.CheckDetails.Keys)
                {
                    integrityModel.CheckDetails[key].checkOk = RunDatabaseCheck(connection, commandExecutor, configurations, outputDestination, key, integrityModel.CheckDetails[key].SqlText, integrityModel.CheckDetails[key].ErrorMessage,
                        integrityModel.CheckDetails[key].returnsRecords, oracleAdminSchema: oracleAdminSchema);
                    FileLogger.TraceLog(String.Format("Check {0} OK result: {1}", key, integrityModel.CheckDetails[key].checkOk));
                }
            } 

            ServerChecks[moduleConnectionStrings_check] = CheckAppsConnectionStrings(dbEngine, outputDestination, moduleConnectionStrings_check);
            FileLogger.TraceLog("Check all modules connection strings OK result: " + ServerChecks[moduleConnectionStrings_check]);

            ServerChecks[systemComponentsInstalled_check] = CheckSystemComponentsInstalled(outputDestination, systemComponentsInstalled_check);
            FileLogger.TraceLog("Check all System Components installed on the server OK result: " + ServerChecks[systemComponentsInstalled_check]);

        }

        private static bool RunDatabaseCheck(IDatabaseConnection connection, IDatabaseCommand commandExecutor, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, string sql,
            string openingErrorMessage, bool sqlResultReturnsRecords, string oracleAdminSchema = null)
        {
            sqlResult = commandExecutor.ReadData(sql, configurations, connection, oracleAdminSchema).Select(row => row.ToList()).ToList();


            if (!sqlResultReturnsRecords && sqlResult.Count.Equals(0))
            {
                return true;
            } else if (sqlResultReturnsRecords && sqlResult.Count >= 1)
            {
                return true;
            }

            Integrity.IntegrityHelper.IntegrityFileWriter(outputDestination, check, openingErrorMessage, new List<string> { GetUpdatedErrorMessages(check) });


            return false;

        }

        private static bool? CheckSystemComponentsInstalled(string outputDestination, string check)
        {
            Dictionary<string, bool> SystemComponentNotInstalledDict = new Dictionary<string, bool>();

            foreach (string systemComponent in Integrity.IntegrityHelper.SystemComponentNames)
            {
                bool systemComponentInstalled = false;

                foreach (KeyValuePair<string, Dictionary<string, DateTime>> runningModule in allRunningFolders)
                {
                    allRunningFolders.TryGetValue(runningModule.Key, out Dictionary<string, DateTime> innerDict);
                    if (systemComponent.Equals(innerDict.Keys.FirstOrDefault())){
                        systemComponentInstalled = true;
                        break;
                    }
                }

                if (!systemComponentInstalled) { SystemComponentNotInstalledDict.Add(systemComponent, !systemComponentInstalled); };

            }

            if (SystemComponentNotInstalledDict.Count.Equals(0))
            {
                return true;
            } else
            {
                using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(outputDestination, check + ".txt"))))
                {
                    writer.WriteLine("== Some Platform System Components were found to not be installed on this server. All System Components must be installed in all servers of the environment. Please check the details below ==" + Environment.NewLine + Environment.NewLine +
                        "* Steps to fix this:" + Environment.NewLine + 
                        "\t 1. Validate if the System Components solution is for the Platform server version installed on the environment and if it includes the components listed below" + Environment.NewLine +
                        "\t \t 1.1 Publish the correct System Component if point 1 is not met and republish the factory" + Environment.NewLine +
                        "\t 2. Validate if all the System Components applications are deployed in a deployment zone that includes all servers of the environment. Please check the documentation in 2.1 for more details" + Environment.NewLine +
                        "\t \t 2.1 https://success.outsystems.com/documentation/11/managing_the_applications_lifecycle/deploy_applications/selective_deployment_using_deployment_zones/" + Environment.NewLine + Environment.NewLine +
                        "* Please check the list of System Component modules that were not found to be installed on this server:");

                    foreach (KeyValuePair<string, bool> notInstalledComponent in SystemComponentNotInstalledDict)
                    {
                        writer.WriteLine("\t - " + notInstalledComponent.Key + " was not found in the Platform running folder");
                    }
                }

                return false;
            }
        }

        private static bool? CheckAppsConnectionStrings(Database.DatabaseType dbEngine, string outputDestination, string check)
        {
            bool equalConnectionString = false;
            List<string> differentConnectionStrings = new List<string>();
            string[] connectionStringList =  { _runtimeConnectionStringKey, _sessionConnectionStringKey, _loggingConnectingStringKey };
            string pKey = Utils.CryptoUtils.GetPrivateKeyFromFile(Program.privateKeyFilepath);

            // Load connection string from each module in running folder
            Dictionary<string, Dictionary<string, string>> modulesConnectionStrings = Integrity.IntegrityHelper.GetModuleConnectionStrings(allRunningFolders, _appSettingsConfig, connectionStringList, pKey) ; // <module name>, [<connection string name>, <connection string value>]
            Dictionary<string, string> platformConnectionStrings = Integrity.IntegrityHelper.GetPlatformConnectionStrings(pKey); // <connection name>, <connection string>
           
            platformConnectionStrings.TryGetValue("RuntimeConnection", out string platformRuntimeConnectionString);
            Dictionary<string, string> platformRuntimeCSProperties = Integrity.IntegrityHelper.ConnectionStringParser(dbEngine, platformRuntimeConnectionString);

            platformConnectionStrings.TryGetValue("LoggingConnection", out string platformLoggingConnectionString);
            Dictionary<string, string> platformLoggingCSProperties = Integrity.IntegrityHelper.ConnectionStringParser(dbEngine, platformLoggingConnectionString);

            platformConnectionStrings.TryGetValue("SessionConnection", out string platformSessionConnectionString);
            Dictionary<string, string> platformSessionCSProperties = Integrity.IntegrityHelper.ConnectionStringParser(dbEngine, platformSessionConnectionString);
            
            // Check each connection string and compare with what is defined on server.hsconf TBC
            foreach (KeyValuePair<string, Dictionary<string,string>> moduleConnections in modulesConnectionStrings)
            {
                modulesConnectionStrings.TryGetValue(moduleConnections.Key, out Dictionary<string,string> innerConnections);

                foreach (KeyValuePair<string,string> connection in innerConnections)
                {
                    Dictionary<string, string> moduleConnectionStringProperties = Integrity.IntegrityHelper.ConnectionStringParser(dbEngine, connection.Value);
                    
                    if (connection.Key.Equals(_runtimeConnectionStringKey)) {
                        equalConnectionString = platformRuntimeCSProperties.OrderBy(kvp => kvp.Key).SequenceEqual(moduleConnectionStringProperties.OrderBy(kvp => kvp.Key));

                        if (!equalConnectionString)
                        {
                            List<string> connStringDiffs = Integrity.IntegrityHelper.ConnStringDifferenceFinder(moduleConnectionStringProperties, platformRuntimeCSProperties);
                            differentConnectionStrings.Add(String.Format("Module {0} {1} was found with a different connection string: " + Environment.NewLine + "-{2}",
                                moduleConnections.Key /*module name*/, connection.Key /*connection name*/, String.Join(Environment.NewLine+"-", connStringDiffs)));
                        }

                    } else if (connection.Key.Equals(_sessionConnectionStringKey))
                    {
                        equalConnectionString = platformSessionCSProperties.OrderBy(kvp => kvp.Key).SequenceEqual(moduleConnectionStringProperties.OrderBy(kvp => kvp.Key));

                        if (!equalConnectionString)
                        {
                            List<string> connStringDiffs = Integrity.IntegrityHelper.ConnStringDifferenceFinder(moduleConnectionStringProperties, platformSessionCSProperties);
                            differentConnectionStrings.Add(String.Format("Module {0} {1} was found with a different connection string: " + Environment.NewLine + "-{2}",
                                moduleConnections.Key /*module name*/, connection.Key /*connection name*/, String.Join(Environment.NewLine + "-", connStringDiffs)));
                        }

                    } else if (connection.Key.Equals(_loggingConnectingStringKey))
                    {
                        equalConnectionString = platformLoggingCSProperties.OrderBy(kvp => kvp.Key).SequenceEqual(moduleConnectionStringProperties.OrderBy(kvp => kvp.Key));

                        if (!equalConnectionString)
                        {
                            List<string> connStringDiffs = Integrity.IntegrityHelper.ConnStringDifferenceFinder(moduleConnectionStringProperties, platformLoggingCSProperties);
                            differentConnectionStrings.Add(String.Format("Module {0} {1} was found with a different connection string: " + Environment.NewLine + "-{2}",
                                moduleConnections.Key /*module name*/, connection.Key /*connection name*/, String.Join(Environment.NewLine + "-", connStringDiffs)));
                        }
                    }
                }
            }

            if (differentConnectionStrings.Count.Equals(0))
            {
                return  true;
            }
            else
            {
                using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(outputDestination, check + ".txt"))))
                {
                    writer.WriteLine("== Module connection strings were found to be different than what was defined in Configuration Tool. Please check the details below ==");
                    foreach (string error in differentConnectionStrings)
                    {
                        writer.WriteLine(error + Environment.NewLine);
                    }
                }

                return false;
            }
        }

        private static string GetUpdatedErrorMessages(string check)
        {
            switch (check)
            {
                case var _ when check.Equals(rpm_4012_espace_check):
                    return String.Format("* Missing eSpace Ids in OSSYS_MODULE table: {0}", String.Join(", ", sqlResult.Select(row => row[0].ToString())));
                case var _ when check.Equals(rpm_4012_extension_check):
                    return String.Format("* Missing extension Ids in OSSYS_MODULE table: {0}", String.Join(", ", sqlResult.Select(row => row[0].ToString())));
                case var _ when check.Equals(devs_Tenants_check):
                    return String.Format("* User Id of developers with wrong tenants: " + Environment.NewLine + String.Join(Environment.NewLine, sqlResult.Select(row => $"- User ID {row[0].ToString()} with tenant {row[1].ToString()}")));
                case var _ when check.Equals(modulesLinkedToApp_check):
                    return "List of modules with no application associated:" + Environment.NewLine + String.Join(Environment.NewLine, sqlResult.Select(row => $"- Module ID: {row[0].ToString()}; eSpace ID: {row[1].ToString()}; Extension ID: {row[2].ToString()}"));
                case var _ when check.Equals(duplicateUsernames_check):
                    return "List of duplicate users found:" + Environment.NewLine + String.Join(Environment.NewLine, sqlResult.Select(row => $"- Username '{row[0].ToString()}' with tenant {row[1].ToString()} was found {row[2].ToString()} times"));
                default:
                    return null;
            }
        }
    }
}
