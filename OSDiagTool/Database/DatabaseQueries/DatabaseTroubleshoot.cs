using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.DatabaseExporter;


namespace OSDiagTool.Database.DatabaseQueries {
    class DatabaseTroubleshoot {

        public static void DatabaseTroubleshooting(string dbEngineType, int queryTimeout, string outputDestination, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null) {

            // Needs user with sa permissions

            int top_statCachedPlan = 10;
            List<string> blockingAndBlockedSpids = new List<string>();

            // work in progress; still needs tests
            if (dbEngineType.ToLower().Equals("sqlserver")) {

                var sqlDBQueries = new SQLServerQueries();

                Dictionary<string, string> sqlQueries = new Dictionary<string, string> { // TODO: use reflection to get the property names
                    { "sessionsSp_Who2", sqlDBQueries.sessionsSp_Who2 },
                    { "sessionsSp_Who2_Blocked", sqlDBQueries.sessionsSp_Who2_Blocked },
                    { "statCachedPlan", string.Format(sqlDBQueries.statCachedPlans, top_statCachedPlan) },
                    { "costlyCPUQueries", sqlDBQueries.costlyCPUQueries },
                    { "dbccInputBuffer", sqlDBQueries.dbccInputBuffer } 
                };

                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);

                using (connection) {

                    foreach (KeyValuePair<string, string> entry in sqlQueries) {

                        if (!(entry.Key.Equals("sessionsSp_Who2_Blocked") || entry.Key.Equals("dbccInputBuffer"))) { // skip sp_who2_blocked and dbcc since it already exports the entire result set of sp_who2
                            CSVExporter.SQLToCSVExport(connection, entry.Key, outputDestination, queryTimeout, entry.Value);
                        } 
                        else if(entry.Key.Equals("sessionsSp_Who2_Blocked")) {

                            SqlCommand cmd = new SqlCommand(entry.Value, connection) {
                                CommandTimeout = queryTimeout
                            };

                            SqlDataReader dr = cmd.ExecuteReader();

                                if (dr.HasRows) {

                                    while (dr.Read()) {

                                        blockingAndBlockedSpids.Add(dr.GetValue(0).ToString()); // add blockings spids to list
                                        blockingAndBlockedSpids.Add(dr.GetValue(1).ToString()); // add blocked spids to list

                                    }

                                }                             

                        }
                        else if (entry.Key.Equals("dbccInputBuffer")) {

                            if (!(blockingAndBlockedSpids.Count.Equals(0))) { // get sql text of blocked and blockig spids

                                string allBlockedSpidsInline = string.Join(",", blockingAndBlockedSpids.ToArray());
                                string blockedSqlTextQuery = string.Format(entry.Value, allBlockedSpidsInline);

                                CSVExporter.SQLToCSVExport(connection, entry.Key, outputDestination, queryTimeout, blockedSqlTextQuery);

                            }
                        }
                    };
                }


            }
            else if (dbEngineType.ToLower().Equals("oracle")) {

                var oracleDBQueries = new OracleQueries();

                string sessionByReadIO = string.Format(oracleDBQueries.sessionByIOType, "DISK_READS");
                string sessionByWriteIO = string.Format(oracleDBQueries.sessionByIOType, "DIRECT_WRITES");


            }





        }





    }
}
