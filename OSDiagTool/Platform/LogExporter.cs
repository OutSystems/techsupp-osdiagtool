using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDiagTool.DatabaseExporter;
using System.IO;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;

namespace OSDiagTool.Platform {
    class LogExporter {

        public static void PlatformLogExporter(string dbEngine, List<string> tableNames , OSDiagToolConf.ConfModel.strConfModel configurations, string outputDestination, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null) {

            if (dbEngine.ToLower().Equals("sqlserver")) {

                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);

                using (connection) {

                    foreach (string table in tableNames) {

                        string sqlQuery = "SELECT TOP {0} * FROM {1} ORDER BY INSTANT DESC";
                        sqlQuery = string.Format(sqlQuery, configurations.osLogTopRecords, table);

                        CSVExporter.SQLToCSVExport(dbEngine, table, Path.Combine(outputDestination), configurations.queryTimeout, sqlQuery, connection, null);

                    }
                }

            } else if (dbEngine.ToLower().Equals("oracle")){

                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(OracleConnectionString);

                using (connection) {

                    foreach (string table in tableNames) {

                        string oracleQuery = "SELECT * FROM (SELECT * FROM {0} ORDER BY INSTANT DESC) WHERE ROWNUM < {1}";
                        oracleQuery = string.Format(oracleQuery, table, configurations.osLogTopRecords);

                        CSVExporter.SQLToCSVExport(dbEngine, table, Path.Combine(outputDestination, table), configurations.queryTimeout, oracleQuery, null, connection);

                    }
                }
            }
        }
    }
}
