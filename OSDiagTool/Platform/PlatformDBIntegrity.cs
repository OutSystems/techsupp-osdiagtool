using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDiagTool.DBConnector;

namespace OSDiagTool.Platform
{
    class PlatformDBIntegrity
    {
        public static void RunDBIntegrityCheck(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination,  DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null)
        {
            List<string> checks = new List<string>()
            {
                "RPM-4012"
            };

            CheckModulesMapping(dbEngine, configurations, SQLConnectionString, OracleConnectionString);

        }


        private static void CheckModulesMapping(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null)
        {
            List<List<object>> ossys_module = new List<List<object>>();
            List<List<object>> ossys_extension = new List<List<object>>();
            List<List<object>> ossys_espace = new List<List<object>>();

            IDatabaseConnection connection = DatabaseConnectionFactory.GetDatabaseConnection(dbEngine, SQLConnectionString, OracleConnectionString);

            using (connection)
            {
                IDatabaseCommand commandExecutor = DatabaseCommandFactory.GetCommandExecutor(dbEngine, connection);
                ossys_module = commandExecutor.ReadData("SELECT ESPACE_ID, EXTENSION_ID FROM OSSYS_MODULE;", configurations, connection).Select(row => row.ToList()).ToList();
                ossys_extension = commandExecutor.ReadData("SELECT ID FROM OSSYS_EXTENSION WHERE IS_ACTIVE=1;", configurations, connection).Select(row => row.ToList()).ToList();
                ossys_espace = commandExecutor.ReadData("SELECT ID FROM OSSYS_ESPACE WHERE IS_ACTIVE=1;", configurations, connection).Select(row => row.ToList()).ToList();
            }

            for (int i = 0; i < ossys_extension.Count; i++)
            {
                List<object> rowData = ossys_extension[i];
                //TBC mapping verifications
            }

        }

    }

}
