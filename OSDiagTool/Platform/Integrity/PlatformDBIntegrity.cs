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
                { "RPM-4012-OK", false }, // modules are mapped to an app ?
                { "Devs_Tenant-OK", false },
                { "ModulesConnectionStrings-OK", false },
            };

        public static void RunDBIntegrityCheck(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination,  DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null)
        {
            bool modulesMappingOK = CheckModulesMappingOK(dbEngine, configurations, outputDestination, "RPM-4012-OK", SQLConnectionString, OracleConnectionString);
            FileLogger.TraceLog("Check modules mapping OK result: " + modulesMappingOK);

            bool devsTenantsOK = CheckDevelopersTenantOK(dbEngine, configurations, outputDestination, "Devs_Tenant-OK", SQLConnectionString, OracleConnectionString);
            FileLogger.TraceLog("Check developers tenant OK result: " + devsTenantsOK);

            CheckAppsConnectionStrings(dbEngine, configurations, outputDestination, "ModulesConnectionStrings-OK", SQLConnectionString, OracleConnectionString);

        }

        // This method checks if all extensions and espaces have a corresponding entry in the OSSYS_MODULE 
        private static bool CheckModulesMappingOK(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null)
        {
            List<List<object>> ossys_module = new List<List<object>>();
            List<List<object>> ossys_extension = new List<List<object>>();
            List<List<object>> ossys_espace = new List<List<object>>();
            List<int> missingExtensionIds = new List<int>();
            List<int> missingESpaceIds = new List<int>();

            IDatabaseConnection connection = DatabaseConnectionFactory.GetDatabaseConnection(dbEngine, SQLConnectionString, OracleConnectionString);

            using (connection)
            {
                IDatabaseCommand commandExecutor = DatabaseCommandFactory.GetCommandExecutor(dbEngine, connection);
                ossys_module = commandExecutor.ReadData("SELECT ESPACE_ID, EXTENSION_ID FROM OSSYS_MODULE;", configurations, connection).Select(row => row.ToList()).ToList();
                ossys_extension = commandExecutor.ReadData("SELECT ID FROM OSSYS_EXTENSION WHERE IS_ACTIVE=1;", configurations, connection).Select(row => row.ToList()).ToList();
                ossys_espace = commandExecutor.ReadData("SELECT ID FROM OSSYS_ESPACE WHERE IS_ACTIVE=1;", configurations, connection).Select(row => row.ToList()).ToList();
            }

            // Check if ExtensionID exists in OSSYS_MODULE
            for (int i = 0; i < ossys_extension.Count; i++)
            {
                object extensionId = ossys_extension[i][0];
                bool extensionIdExists = false;

                for (int j=0; j < ossys_module.Count; j++)
                {
                    object modExtId = ossys_module[j][1];

                    if (extensionId.Equals(modExtId))
                    {
                        extensionIdExists = true;
                        break;
                    }
                }

                if (extensionIdExists.Equals(false))
                {
                    missingExtensionIds.Add((int)extensionId);
                }
                
            }

            // Check if eSpaceId exists in OSSYS_MODULE
            for (int i = 0; i < ossys_espace.Count; i++)
            {
                object espaceId = ossys_espace[i][0];
                bool eSpaceIdExists = false;

                for (int j = 0; j < ossys_espace.Count; j++)
                {
                    object modESpaceId = ossys_espace[j][0];

                    if (espaceId.Equals(modESpaceId))
                    {
                        eSpaceIdExists = true;
                        break;
                    }
                }

                if (eSpaceIdExists.Equals(false))
                {
                    missingESpaceIds.Add((int)espaceId);
                }
            }

            if (missingESpaceIds.Count.Equals(0) && missingExtensionIds.Count.Equals(0))
            {
                return checks[check] = true;
            }

            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(outputDestination, check + ".txt"))))
            {
                writer.WriteLine(string.Format("== A Platform integrty issue was found - please open a support case to follow up and provide the output of OSDiagTool ==" + Environment.NewLine));
                writer.WriteLine(string.Format("* Missing eSpace Ids in OSSYS_MODULE table: {0}", String.Join(", ", missingESpaceIds)));
                writer.WriteLine(string.Format("* Missing extension Ids in OSSYS_MODULE table: {0}", String.Join(", ", missingExtensionIds)));
            }

            return checks[check] = false;
        }

        // This method cross references the User_ID of OSSYS_USER_DEVELOPER with the User_Id of OSSYS_USER and checks if the users have Service Center tenant (1)
        private static bool CheckDevelopersTenantOK(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null)
        {
            List<List<object>> ossys_user = new List<List<object>>();
            List<List<object>> ossys_user_developer = new List<List<object>>();
            Dictionary<int, int> devsWithWrongTenant = new Dictionary<int,int>();

            IDatabaseConnection connection = DatabaseConnectionFactory.GetDatabaseConnection(dbEngine, SQLConnectionString, OracleConnectionString);

            using (connection)
            {
                IDatabaseCommand commandExecutor = DatabaseCommandFactory.GetCommandExecutor(dbEngine, connection);

                ossys_user_developer = commandExecutor.ReadData("SELECT DISTINCT USER_ID FROM OSSYS_USER_DEVELOPER;", configurations, connection).Select(row => row.ToList()).ToList();
                List<string> userIds = ossys_user_developer.SelectMany(innerList => innerList.Select(item => item.ToString())).ToList();
                string formattedUserIds = String.Join(", ", userIds);

                ossys_user = commandExecutor.ReadData(String.Format("SELECT ID, TENANT_ID FROM OSSYS_USER WHERE IS_ACTIVE=1 AND ID IN ({0});", formattedUserIds), configurations, connection).Select(row => row.ToList()).ToList();
            }

            for (int i = 0; i < ossys_user_developer.Count; i++)
            {
                object user_devId = ossys_user_developer[i][0];

                for (int j = 0; j < ossys_user.Count; j++)
                {
                    object userId = ossys_user[j][0];
                    if (userId.Equals(user_devId))
                    {
                        object tenantId = ossys_user[j][1];
                        if (!tenantId.Equals(1))
                        {
                            devsWithWrongTenant.Add((int)user_devId, (int)tenantId);
                        }
                    } 
                }
            }

            if (devsWithWrongTenant.Count.Equals(0))
            {
                return checks[check] = true;
            }

            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(outputDestination, check + ".txt"))))
            {
                writer.WriteLine(string.Format("== A Platform integrty issue was found - Developers with wront tenants (expected tenant ID 1) were found - please open a support case to follow up and provide the output of OSDiagTool ==" + Environment.NewLine));
                writer.WriteLine(string.Format("* User Id of developers with wrong tenants: " + Environment.NewLine + String.Join(Environment.NewLine, devsWithWrongTenant.Select(kv => $"- User ID {kv.Key} with tenant {kv.Value}"))));
            }

            return checks[check] = false;
        }

        private static bool CheckAppsConnectionStrings(Database.DatabaseType dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null)
        {
            string[] connectionStringList =  { _runtimeConnectionStringKey, _sessionConnectionStringKey, _loggingConnectingStringKey };
            string platformRunningPath = Path.Combine(Program._osInstallationFolder, "running");

            

            // Load connection string from each module in running folder
            Dictionary<string, Dictionary<string, DateTime>> allRunningFolders = Integrity.IntegrityHelper.GetLastModulesRunningPublished(platformRunningPath); // <path folder>, [<module name>, <creation date>] - check for duplicates and use latest created only            
            Dictionary<string, Dictionary<string, string>> modulesConnectionStrings = Integrity.IntegrityHelper.GetModuleConnectionStrings(allRunningFolders, _appSettingsConfig, connectionStringList) ; // <path folder>, [<connection string name>, <connection string value>]

            // Check each connection string and compare with what is defined on server.hsconf TBC
            foreach (KeyValuePair<string, Dictionary<string,string>> moduleConnections in modulesConnectionStrings)
            {

            }


            return false;
        }
    }
}
