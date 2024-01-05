﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDiagTool.DBConnector;

namespace OSDiagTool.Platform
{
    public class PlatformDBIntegrity
    {
        public static Dictionary<string, bool> checks = new Dictionary<string, bool>()
            {
                { "RPM-4012-OK", false },
            };

        public static void RunDBIntegrityCheck(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination,  DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null)
        {
            bool ModulesMappingOK = CheckModulesMappingOK(dbEngine, configurations, outputDestination, "RPM-4012-OK", SQLConnectionString: SQLConnectionString, OracleConnectionString: OracleConnectionString);
            FileLogger.TraceLog("Check modules mapping OK result: " + ModulesMappingOK);

        }


        private static bool CheckModulesMappingOK(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, string check, DBConnector.SQLConnStringModel SQLConnectionString = null,
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
    }
}
