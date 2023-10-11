using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.DatabaseExporter;
using OSDiagTool.OSDiagToolConf;

namespace OSDiagTool.Database.DatabaseQueries {
    class DatabaseTroubleshoot {

        public static void DatabaseTroubleshooting(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations,string outputDestination, DBConnector.SQLConnStringModel SQLConnectionString = null,
            DBConnector.OracleConnStringModel OracleConnectionString = null) {

            // Needs user with sa permissions

            if (dbEngine.ToLower().Equals("sqlserver")) {

                List<string> blockingAndBlockedSpids = new List<string>();

                int top_statCachedPlan = Convert.ToInt32(configurations.databaseQueryConfigurations[OSDiagToolConfReader._l3_sqlServer][OSDiagToolConfReader._l4_top_statCachedPlans]);
                int top_topCPU = Convert.ToInt32(configurations.databaseQueryConfigurations[OSDiagToolConfReader._l3_sqlServer][OSDiagToolConfReader._l4_top_topCPU]);

                var sqlDBQueries = new SQLServerQueries();

                Dictionary<string, string> sqlQueries = new Dictionary<string, string> { // use same name as in the DatabaseQueries
                    { "sessionsSp_Who2", sqlDBQueries.sessionsSp_Who2 },
                    { "sessionsSp_Who2_Blocked", sqlDBQueries.sessionsSp_Who2_Blocked },
                    { "statCachedPlan", string.Format(sqlDBQueries.statCachedPlans, top_statCachedPlan) },
                    { "costlyCPUQueries", string.Format(sqlDBQueries.costlyCPUQueries, top_topCPU) },
                    { "dbccInputBuffer", sqlDBQueries.dbccInputBuffer },
                    { "sessionsLocksTree", sqlDBQueries.sessionsLocksTree },
                    { "locksAssociatedResources", sqlDBQueries.locksAssociatedResources }
                };

                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);

                using (connection) {

                    foreach (KeyValuePair<string, string> entry in sqlQueries) {

                        if (!(entry.Key.Equals("sessionsSp_Who2_Blocked") || entry.Key.Equals("dbccInputBuffer"))) { // skip sp_who2_blocked and dbcc since it already exports the entire result set of sp_who2

                            CSVExporter.SQLToCSVExport(dbEngine, entry.Key, outputDestination, configurations.queryTimeout, entry.Value, connection, null);

                        } else if(entry.Key.Equals("sessionsSp_Who2_Blocked")) {

                            SqlCommand cmd = new SqlCommand(entry.Value, connection) {
                                CommandTimeout = configurations.queryTimeout
                            };

                            SqlDataReader dr = cmd.ExecuteReader();

                                if (dr.HasRows) {

                                    while (dr.Read()) {

                                        blockingAndBlockedSpids.Add(dr.GetValue(0).ToString()); // add blockings spids to list
                                        blockingAndBlockedSpids.Add(dr.GetValue(1).ToString()); // add blocked spids to list

                                    }

                                }                             

                        } else if (entry.Key.Equals("dbccInputBuffer")) {

                            if (!(blockingAndBlockedSpids.Count.Equals(0))) { // get sql text of blocked and blockig spids

                                string allBlockedSpidsInline = string.Join(",", blockingAndBlockedSpids.ToArray());
                                string blockedSqlTextQuery = string.Format(entry.Value, allBlockedSpidsInline);

                                CSVExporter.SQLToCSVExport(dbEngine, entry.Key, outputDestination, configurations.queryTimeout, blockedSqlTextQuery, connection, null);

                            }
                        }
                    };
                }


            } else if (dbEngine.ToLower().Equals("oracle")) {

                List<string> orclSids = new List<string>();

                int orcl_TopCPU = Convert.ToInt32(configurations.databaseQueryConfigurations[OSDiagToolConfReader._l3_oracle][OSDiagToolConfReader._l4_top_topCPU]);

                var orclDBQueries = new OracleQueries();

                Dictionary<string, string> orclQueries = new Dictionary<string, string> { // TODO: use reflection to get the property names
                    { "orcl_lockedObjects", orclDBQueries.lockedObjects },
                    { "orcl_lockedObjects_2", orclDBQueries.lockedObjects_2 },
                    { "orcl_resourceLimit", orclDBQueries.resourceLimit },
                    { "orcl_sessionByIOType_Reads", string.Format(orclDBQueries.sessionByIOType, "DISK_READS") },
                    { "orcl_sessionByIOType_Writes", string.Format(orclDBQueries.sessionByIOType, "DIRECT_WRITES") },
                    { "orcl_tk_queriesRunningNow", orclDBQueries.tk_queriesRunningNow },
                    { "orcl_sqlTextBySID", orclDBQueries.sqlTextBySID },
                    { "orcl_sidInfo", orclDBQueries.sidInfo },
                    { "orcl_topCPUSqls", string.Format(orclDBQueries.topCPUSqls, orcl_TopCPU) }
                };

                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(OracleConnectionString);

                using (connection) {

                    foreach (KeyValuePair<string, string> entry in orclQueries) {

                        if (!(entry.Key.Equals("orcl_lockedObjects") || (entry.Key.Equals("orcl_sessionByIOType_Writes") || (entry.Key.Equals("orcl_sessionByIOType_Reads") || (entry.Key.Equals("orcl_lockedObjects_2") ||
                            (entry.Key.Equals("orcl_sqlTextBySID") || (entry.Key.Equals("orcl_sidInfo")))))))) { // skip queries that we want to know more about the sessions

                            CSVExporter.SQLToCSVExport(dbEngine, entry.Key, outputDestination, configurations.queryTimeout, entry.Value, null, connection);

                        } else if (entry.Key.Equals("orcl_lockedObjects") || entry.Key.Equals("orcl_lockedObjects_2") || entry.Key.Equals("orcl_sessionByIOType_Reads") || entry.Key.Equals("orcl_sessionByIOType_Writes")) {

                            OracleCommand cmd = new OracleCommand(entry.Value, connection) {
                                CommandTimeout = configurations.queryTimeout
                            };

                            OracleDataReader dr = cmd.ExecuteReader();

                            if (dr.HasRows) {

                                while (dr.Read()) {

                                    orclSids.Add(dr.GetValue(0).ToString()); // add Sids to list

                                }

                            }

                            CSVExporter.SQLToCSVExport(dbEngine, entry.Key, outputDestination, configurations.queryTimeout, entry.Value, null, connection);

                        } else if (entry.Key.Equals("orcl_sidInfo") || entry.Key.Equals("orcl_sqlTextBySID")) {

                            if (!(orclSids.Count.Equals(0))) {

                                string allSidsInline = string.Join(",", orclSids.ToArray());
                                string sidsSqlTextQuery = string.Format(entry.Value, allSidsInline);

                                CSVExporter.SQLToCSVExport(dbEngine, entry.Key, outputDestination, configurations.queryTimeout, sidsSqlTextQuery, null, connection);

                            }

                        }

                    }

                }

            }





        }





    }
}
