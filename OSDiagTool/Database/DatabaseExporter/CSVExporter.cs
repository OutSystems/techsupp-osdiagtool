using System;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;

namespace OSDiagTool.DatabaseExporter {
    class CSVExporter {
        
        public static void SQLToCSVExport(string dbEngine, string tableName, string csvFilePath, int queryTimeout, string query, SqlConnection SqlConnection = null, OracleConnection Orclconnection = null) {

            try {

                using (System.IO.StreamWriter fs = new System.IO.StreamWriter(csvFilePath + "\\" + tableName + ".csv")) {

                    if (dbEngine.ToLower().Equals("sqlserver")) {

                        SqlCommand command = new SqlCommand(query, SqlConnection) {
                            CommandTimeout = queryTimeout
                        };

                        SqlDataReader dr = command.ExecuteReader();

                        for (int i = 0; i < dr.FieldCount; i++) {

                            var name = dr.GetName(i);

                            if (name.Contains(",")) {
                                name = name.Replace(",", "_"); // replacing commas for _ to avoid writting in a next cell
                            }

                            fs.Write(name + ",");
                        }

                        fs.WriteLine();

                        while (dr.Read()) {
                            for (int i = 0; i < dr.FieldCount; i++) {

                                var value = dr[i].ToString();
                                value = Regex.Replace(value, @"(;|\r\n|\n)", " "); // replacing semicolon and new lines


                                if (value.Contains(",")) {
                                    value = value.Replace(",", "_"); // replacing commas for _ to avoid writting in a next cell
                                }

                                fs.Write(value + ",");
                            }

                            fs.WriteLine();
                        }

                    } else if(dbEngine.ToLower().Equals("oracle")) { // oracle

                        OracleCommand command = new OracleCommand(query, Orclconnection) {
                            CommandTimeout = queryTimeout
                        };

                        OracleDataReader dr = command.ExecuteReader();

                        for (int i = 0; i < dr.FieldCount; i++) {
                            var name = dr.GetName(i);

                            if (name.Contains(",")) {
                                name = name.Replace(",", "_"); // replacing commas for _ to avoid writting in a next cell
                            }

                            fs.Write(name + ",");
                        }

                        fs.WriteLine();

                        while (dr.Read()) {
                            for (int i = 0; i < dr.FieldCount; i++) {

                                var value = dr[i].ToString();

                                value = Regex.Replace(value, @"(;|\r\n|\n)", " "); // replacing semicolon and new lines

                                if (value.Contains(",")) {
                                    value = value.Replace(",", "_"); // replacing commas for _ to avoid writting in a next cell
                                }

                                fs.Write(value + ",");
                            }

                            fs.WriteLine();
                        }

                    }

                }

                } catch (Exception e) {
                FileLogger.LogError("Unable to read data from SQL DB: " + tableName, e.Message + e.StackTrace, writeToConsole: false, writeDateTime: false);
                }


        }



        public static void ORCLToCsvExport(OracleConnection connection, string tableName, string csvFilePath, int queryTimeout, string osAdminSchema, string query) {

            using (System.IO.StreamWriter fs = new System.IO.StreamWriter(csvFilePath + "\\" + tableName + ".csv")) {

                OracleCommand command = new OracleCommand(query, connection) {
                    CommandTimeout = queryTimeout
                };

                try {

                    OracleDataReader dr = command.ExecuteReader();

                    for (int i = 0; i < dr.FieldCount; i++) {
                        string name = dr.GetName(i);

                        if (name.Contains(",")) {
                            name = name.Replace(",", "_"); // replacing commas for _ to avoid writting in a next cell
                        }

                        fs.Write(name + ",");
                    }

                    fs.WriteLine();

                    while (dr.Read()) {
                        for (int i = 0; i < dr.FieldCount; i++) {
                            string value = dr[i].ToString();

                            if (value.Contains(",")) {
                                value = value.Replace(",", "_"); // replacing commas for _ to avoid writting in a next cell
                            }

                            fs.Write(value + ",");
                        }
                        fs.WriteLine();
                    }

                } catch (Exception e) {
                    FileLogger.LogError("Unable to read data from SQL DB: " + tableName, e.Message + e.StackTrace, writeToConsole: false, writeDateTime: false);
                }

            }


        }

    }
}
