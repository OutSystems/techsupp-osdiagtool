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

        public static void PlatformLogExporter(string dbEngine, List<string> tableNames, OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations, string outputDestination, int queryTimeout, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null, string adminSchema = null) {

            if (dbEngine.ToLower().Equals("sqlserver")) {

                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);

                using (connection) {

                    foreach (string table in tableNames) {

                        string sqlQuery = "SELECT TOP {0} * FROM {1} ORDER BY INSTANT DESC";
                        sqlQuery = string.Format(sqlQuery, FormConfigurations.osLogTopRecords, table);

                        FileLogger.TraceLog(string.Format("Exporting log table {0} ", table));

                        CSVExporter.SQLToCSVExport(dbEngine, table, Path.Combine(outputDestination), queryTimeout, sqlQuery, connection, null);

                    }
                }

            } else if (dbEngine.ToLower().Equals("oracle")){

                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(OracleConnectionString);

                using (connection) {

                    foreach (string table in tableNames) {

                        string oracleQuery = "SELECT * FROM (SELECT * FROM {0}.{1} ORDER BY INSTANT DESC) WHERE ROWNUM < {2}";
                        oracleQuery = string.Format(oracleQuery, adminSchema, table, FormConfigurations.osLogTopRecords);

                        FileLogger.TraceLog(string.Format("Exporting log table {0} ", table));

                        CSVExporter.SQLToCSVExport(dbEngine, table, outputDestination, queryTimeout, oracleQuery, null, connection);

                    }
                }
            }
        }
    }
}
