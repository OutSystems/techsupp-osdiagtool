using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OSDiagTool.DBConnector;

namespace OSDiagTool.Platform
{
    public class PlatformDBIntegrity
    {
        private static string _appSettingsConfig = "appSettings.config";
        private static string _runtimeConnectionStringKey = "OutSystems.DB.Platform.Application.Runtime.ConnectionString (DEFAULT)";
        private static string _sessionConnectionStringKey = "OutSystems.DB.Platform.Application.Session.ConnectionString (DEFAULT)";
        private static string _loggingConnectingStringKey = "OutSystems.DB.Logging.Application.Runtime.ConnectionString (DEFAULT)";

        public static Dictionary<string, bool> checks = new Dictionary<string, bool>()
            {
                { "RPM-4012-OK", false }, // espaces and extensions exist in ossys_module?
                { "Devs_Tenant-OK", false }, // developers have Service Center tenant (1)?
                { "ModulesConnectionStrings-OK", false }, // modules have updated connection strings?
            };

        public static void RunIntegrityCheck(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination,  DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string oracleAdminSchema = null)
        {
            bool modulesMappingOK = CheckModulesMappingOK(dbEngine, configurations, outputDestination, "RPM-4012-OK", SQLConnectionString, OracleConnectionString, oracleAdminSchema);
            FileLogger.TraceLog("Check modules mapping OK result: " + modulesMappingOK);

            bool devsTenantsOK = CheckDevelopersTenantOK(dbEngine, configurations, outputDestination, "Devs_Tenant-OK", SQLConnectionString, OracleConnectionString, oracleAdminSchema);
            FileLogger.TraceLog("Check developers tenant OK result: " + devsTenantsOK);

            CheckAppsConnectionStrings(dbEngine, configurations, outputDestination, "ModulesConnectionStrings-OK");
            FileLogger.TraceLog("Check all modules connection strings OK result: " + checks["ModulesConnectionStrings-OK"]);

        }

        // This method checks if all extensions and espaces have a corresponding entry in the OSSYS_MODULE 
        private static bool CheckModulesMappingOK(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string oracleAdminSchema = null)
        {
            List<List<object>> ossys_extensionMissing = new List<List<object>>();
            List<List<object>> ossys_espaceMissing = new List<List<object>>();

            string espaceMissingSql = @"SELECT E.ID, M.ESPACE_ID
                                    FROM OSSYS_ESPACE E
                                    LEFT JOIN OSSYS_MODULE M ON E.ID = M.ESPACE_ID
                                    WHERE M.ESPACE_ID IS NULL AND E.IS_ACTIVE = 1";

            string extensionMissingSql = @"SELECT E.ID, M.EXTENSION_ID
                                    FROM OSSYS_EXTENSION E
                                    LEFT JOIN OSSYS_MODULE M ON E.ID = M.EXTENSION_ID
                                    WHERE M.EXTENSION_ID IS NULL AND E.IS_ACTIVE = 1";

            IDatabaseConnection connection = DatabaseConnectionFactory.GetDatabaseConnection(dbEngine, SQLConnectionString, OracleConnectionString);

            using (connection)
            {
                IDatabaseCommand commandExecutor = DatabaseCommandFactory.GetCommandExecutor(dbEngine, connection);
                ossys_extensionMissing = commandExecutor.ReadData(extensionMissingSql, configurations, connection, oracleAdminSchema).Select(row => row.ToList()).ToList();
                ossys_espaceMissing = commandExecutor.ReadData(espaceMissingSql, configurations, connection, oracleAdminSchema).Select(row => row.ToList()).ToList();
            }

            if (ossys_extensionMissing.Count.Equals(0) && ossys_espaceMissing.Count.Equals(0))
            {
                return checks[check] = true;
            }

            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(outputDestination, check + ".txt"))))
            {
                writer.WriteLine(string.Format("== A Platform integrty issue was found - please open a support case to follow up and provide the output of OSDiagTool and the contents of the tables OSSYS_ESPACE, OSSYS_EXTENSION and OSSYS_MODULE ==" + Environment.NewLine));
                writer.WriteLine(string.Format("* Missing eSpace Ids in OSSYS_MODULE table: {0}", string.Join(", ", ossys_espaceMissing.Select(row => row[0].ToString()))));
                writer.WriteLine(string.Format("* Missing extensions Ids in OSSYS_MODULE table: {0}", string.Join(", ", ossys_extensionMissing.Select(row => row[0].ToString()))));
            }

            return checks[check] = false;

        }
            

        // This method cross references the User_ID of OSSYS_USER_DEVELOPER with the User_Id of OSSYS_USER and checks if the users have Service Center tenant (1)
        private static bool CheckDevelopersTenantOK(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string oracleAdminSchema = null)
        {
            List<List<object>> ossys_user = new List<List<object>>();
            List<List<object>> devsWithWrongTenant = new List<List<object>>();
            

            string checkDevsTenantsSql = @"SELECT DISTINCT UD.USER_ID, US.TENANT_ID FROM OSSYS_USER_DEVELOPER UD
                                            LEFT JOIN OSSYS_USER US ON UD.USER_ID=US.ID
                                            WHERE US.IS_ACTIVE=1 /*AND US.TENANT_ID <> 1*/";

            IDatabaseConnection connection = DatabaseConnectionFactory.GetDatabaseConnection(dbEngine, SQLConnectionString, OracleConnectionString);

            using (connection)
            {
                IDatabaseCommand commandExecutor = DatabaseCommandFactory.GetCommandExecutor(dbEngine, connection);
                devsWithWrongTenant = commandExecutor.ReadData(checkDevsTenantsSql, configurations, connection, oracleAdminSchema).Select(row => row.ToList()).ToList();
            }

            if (devsWithWrongTenant.Count.Equals(0))
            {
                return checks[check] = true;
            }

            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(outputDestination, check + ".txt"))))
            {
                writer.WriteLine(string.Format("== A Platform integrity issue was found - Developers with wront tenants (expected tenant ID 1) were found - please open a support case to follow up and provide the output of OSDiagTool ==" + Environment.NewLine));
                writer.WriteLine(string.Format("* User Id of developers with wrong tenants: " + Environment.NewLine + String.Join(Environment.NewLine, devsWithWrongTenant.Select(row => $"- User ID {row[0].ToString()} with tenant {row[1].ToString()}"))));
            }

            return checks[check] = false;

        }

        private static bool CheckAppsConnectionStrings(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check)
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
                return checks[check] = true;
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

                return checks[check] = false;
            }
        }
    }
}
