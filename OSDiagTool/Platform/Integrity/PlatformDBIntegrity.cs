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
         * 2. add the static string check key in the metamodelChecks Dictionary;
         * 3. add the static string check key in the List Dictionary checksInfo where [0] is the sql to run the validation and [1] is the opening error message in case an integrity issue is found
         * 4. upddate the GetUpdatedErrorMessages() method to return the information with the details of the metamodel related to the integrity issue
         * 
         * Assumptions:
         * - the sql should validate the integrity by returning no records --> check RunDatabaseCheck() method return
        */
        private static string _appSettingsConfig = "appSettings.config";
        private static string _runtimeConnectionStringKey = "OutSystems.DB.Platform.Application.Runtime.ConnectionString (DEFAULT)";
        private static string _sessionConnectionStringKey = "OutSystems.DB.Platform.Application.Session.ConnectionString (DEFAULT)";
        private static string _loggingConnectingStringKey = "OutSystems.DB.Logging.Application.Runtime.ConnectionString (DEFAULT)";
        private static string rpm_4012_espace_check = "RPM-4012-eSpace";
        private static string rpm_4012_extension_check = "RPM-4012-extension";
        private static string devs_Tenants_check = "Developers_Tenants";
        private static string moduleConnectionStrings_check = "ModuleConnectionStrings";
        private static string modulesLinkedToApp_check = "ModulesLinkedToApp";
        private static string duplicateUsernames_check = "DuplicateUsernames";
        private static List<List<object>> sqlResult = new List<List<object>>();

        public static Dictionary<string, bool> metamodelChecks = new Dictionary<string, bool>()
            {
                { rpm_4012_espace_check, false }, // espaces exist in ossys_module?
                { rpm_4012_extension_check, false }, // extensions exist in ossys_module?
                { devs_Tenants_check, false }, // developers have Service Center tenant (1)?
                { modulesLinkedToApp_check, false }, // modules are associated to an application is OSSYS_APP_DEFINITION_MODULE?
                { duplicateUsernames_check, false }, // duplicate usernames in the same tenant exist?
            };

        public static List<Dictionary<string, string>> checksInfo = new List<Dictionary<string, string>>() { // [0] sql message, [1] openingErrorMessage

            new Dictionary<string, string>()
            {
                { rpm_4012_espace_check,  @"SELECT E.ID, M.ESPACE_ID
                                    FROM OSSYS_ESPACE E
                                    LEFT JOIN OSSYS_MODULE M ON E.ID = M.ESPACE_ID
                                    WHERE M.ESPACE_ID IS NULL AND E.IS_ACTIVE = 1" },
                { rpm_4012_extension_check, @"SELECT E.ID, M.EXTENSION_ID
                                    FROM OSSYS_EXTENSION E
                                    LEFT JOIN OSSYS_MODULE M ON E.ID = M.EXTENSION_ID
                                    WHERE M.EXTENSION_ID IS NULL AND E.IS_ACTIVE = 1" },
                { devs_Tenants_check, @"SELECT DISTINCT UD.USER_ID, US.TENANT_ID FROM OSSYS_USER_DEVELOPER UD
                                            LEFT JOIN OSSYS_USER US ON UD.USER_ID=US.ID
                                            WHERE US.IS_ACTIVE=1 AND US.TENANT_ID <> 1" },
                { modulesLinkedToApp_check, @"SELECT M.ID, M.ESPACE_ID, M.EXTENSION_ID
                                                FROM OSSYS_MODULE M
                                                LEFT JOIN OSSYS_APP_DEFINITION_MODULE APPDEF ON M.ID = APPDEF.MODULE_ID
                                                LEFT JOIN OSSYS_ESPACE E ON M.ESPACE_ID = E.ID
                                                LEFT JOIN OSSYS_EXTENSION EXT ON M.EXTENSION_ID = EXT.ID
                                                WHERE APPDEF.MODULE_ID IS NULL
                                                AND (E.IS_ACTIVE = 1 OR EXT.IS_ACTIVE = 1)" },
                { duplicateUsernames_check, @"SELECT USERNAME, TENANT_ID, COUNT(1)
                                                    FROM OSSYS_USER 
                                                    WHERE IS_ACTIVE = 1
                                                    GROUP BY TENANT_ID, USERNAME
                                                    HAVING COUNT(1) > 1" },
            }, 
            new Dictionary<string, string>()
            {
                { rpm_4012_espace_check,  "== A Platform integrity issue was found - please open a support case to follow up and provide the output of OSDiagTool and the contents of the tables OSSYS_ESPACE and OSSYS_MODULE =="},
                { rpm_4012_extension_check, "== A Platform integrity issue was found - please open a support case to follow up and provide the output of OSDiagTool and the contents of the tables OSSYS_EXTENSION and OSSYS_MODULE ==" },
                { devs_Tenants_check, "== A Platform integrity issue was found - Developers with wront tenants (expected tenant ID 1) were found - please open a support case to follow up and provide the output of OSDiagTool ==" },
                { modulesLinkedToApp_check, "== A Platform integrity issue was found - Modules with no app associated were found - please open a support case to follow up and provide the contents of the tables OSSYS_ESPACE, OSSYS_EXTENSION, OSSYS_MODULE, OSSYS_APP_DEFINITION_MODULE and the output of OSDiagTool ==" },
                { duplicateUsernames_check, "== A Platform integrity issue was found - Duplicate usernames in the same tenant were found - please review your User Management application and review the duplicate users found in OSSYS_USER ==" },
            }
            };

        public static void RunIntegrityCheck(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination,  DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string oracleAdminSchema = null)
        {
            IDatabaseConnection connection = DatabaseConnectionFactory.GetDatabaseConnection(dbEngine, SQLConnectionString, OracleConnectionString);
            IDatabaseCommand commandExecutor = DatabaseCommandFactory.GetCommandExecutor(dbEngine, connection);

            using (connection)
            {
                Dictionary<string, bool> checksCopy = new Dictionary<string, bool> (metamodelChecks);
                foreach (string key in checksCopy.Keys)
                {
                    metamodelChecks[key] = RunDatabaseCheck(connection, commandExecutor, configurations, outputDestination, key, checksInfo[0][key], checksInfo[1][key], oracleAdminSchema: oracleAdminSchema);
                    FileLogger.TraceLog(String.Format("Check {0} OK result: {1}", key, metamodelChecks[key]));
                }
            }

            bool connectionStringsOK = CheckAppsConnectionStrings(dbEngine, outputDestination, moduleConnectionStrings_check);
            FileLogger.TraceLog("Check all modules connection strings OK result: " + connectionStringsOK);

        }

        private static bool RunDatabaseCheck(IDatabaseConnection connection, IDatabaseCommand commandExecutor, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, string sql,
            string openingErrorMessage, string oracleAdminSchema = null)
        {
            sqlResult = commandExecutor.ReadData(sql, configurations, connection, oracleAdminSchema).Select(row => row.ToList()).ToList();

            if (sqlResult.Count.Equals(0))
            {
                return true;
            }

            Integrity.IntegrityHelper.IntegrityFileWriter(outputDestination, check, openingErrorMessage, new List<string> { GetUpdatedErrorMessages(check) });

            return false;
        }

        private static bool CheckAppsConnectionStrings(Database.DatabaseType dbEngine, string outputDestination, string check)
        {
            bool equalConnectionString = false;
            List<string> differentConnectionStrings = new List<string>();
            string[] connectionStringList =  { _runtimeConnectionStringKey, _sessionConnectionStringKey, _loggingConnectingStringKey };
            string platformRunningPath = Path.Combine(Program._osInstallationFolder, "running");
            string pKey = Utils.CryptoUtils.GetPrivateKeyFromFile(Program.privateKeyFilepath);

            // Load connection string from each module in running folder
            Dictionary<string, Dictionary<string, DateTime>> allRunningFolders = Integrity.IntegrityHelper.GetLastModulesRunningPublished(platformRunningPath); // <path folder>, [<module name>, <creation date>] - check for duplicates and use latest created only            
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
                return true;
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
                case var _ when check == rpm_4012_espace_check:
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
