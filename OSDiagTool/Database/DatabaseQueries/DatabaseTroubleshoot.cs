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
            List<int> blockingAndBlockedSpids = new List<int>();

            // work in progress; still needs tests
            if (dbEngineType.ToLower().Equals("sqlserver")) {

                var sqlDBQueries = new SQLServerQueries();

                Dictionary<string, string> sqlQueries = new Dictionary<string, string> { // TODO: use reflection to get the property names
                    { "sessionsSp_Who2", string.Format(sqlDBQueries.sessionsSp_Who2) },
                    { "sessionsSp_Who2_Blocked", string.Format(sqlDBQueries.sessionsSp_Who2_Blocked) },
                    { "statCachedPlan", string.Format(sqlDBQueries.statCachedPlans, top_statCachedPlan) },
                    { "costlyCPUQueries", string.Format(sqlDBQueries.costlyCPUQueries) },
                    { "dbccInputBuffer", string.Format(sqlDBQueries.dbccInputBuffer) } 
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
                                    if (!(dr.GetInt32(1).Equals(0))){ // Check if BlkBy is not zero, hence, not blocked
                                        blockingAndBlockedSpids.Add(dr.GetInt32(0)); // add blockings spids to list
                                        blockingAndBlockedSpids.Add(dr.GetInt32(1)); // add blocked spids to list
                                    };
                                    
                                }
                            }

                        }
                        else if (entry.Key.Equals("dbccInputBuffer")) {

                            if (!(blockingAndBlockedSpids.Count.Equals(0))) { // get sql text of blocked and blockig spids

                                string allBlockedSpidsInline = string.Join(",", blockingAndBlockedSpids.ToArray());
                                string blockedSqlTextQuery = string.Format(entry.Key, allBlockedSpidsInline);

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
