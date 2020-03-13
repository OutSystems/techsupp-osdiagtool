using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.OracleClient;
using System.IO;

namespace OSDiagTool.DatabaseExporter {
    class CSVExporter {

        private static string _selectAllQuery = "SELECT * FROM ";
        
        public static void SQLToCSVExport(DBConnector.SQLConnStringModel SQLConnectionString, string tableName, string csvFilePath, int queryTimeout) {

            var connector = new DBConnector.SLQDBConnector();
            SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);

            using (System.IO.StreamWriter fs = new System.IO.StreamWriter(csvFilePath + "\\" + tableName + ".csv")) {
                               
                using (connection) {
                    _selectAllQuery = "SELECT * FROM " + tableName;
                    SqlCommand command = new SqlCommand(_selectAllQuery, connection);
                    command.CommandTimeout = queryTimeout;

                    try {

                        SqlDataReader dr = command.ExecuteReader();

                        for(int i = 0; i < dr.FieldCount; i++) {
                            string name = dr.GetName(i);

                            fs.Write(name + ";");
                        }

                        fs.WriteLine();

                        while (dr.Read()) {
                            for (int i = 0; i < dr.FieldCount; i++) {
                                string value = dr[i].ToString();

                                fs.Write(value + ";");
                            }
                            fs.WriteLine();
                        }
                        
                    } catch (Exception e){
                        Console.WriteLine("Unable to read data from SQL DB: " + tableName, e);
                    }
                                       
            }
                          

            }
        }

        public static void ORCLToCsvExport(DBConnector.OracleConnStringModel ORCLConnectionString, string tableName, string csvFilePath, int queryTimeout) {

            var connector = new DBConnector.OracleDBConnector();
            OracleConnection connection = connector.OracleOpenConnection(ORCLConnectionString);

            using (System.IO.StreamWriter fs = new System.IO.StreamWriter(csvFilePath + "\\" + tableName + ".csv")) {

                using (connection) {
                    _selectAllQuery = "SELECT * FROM " + tableName;
                    OracleCommand command = new OracleCommand(_selectAllQuery, connection);
                    command.CommandTimeout = queryTimeout;

                    try {

                        OracleDataReader dr = command.ExecuteReader();

                        for (int i = 0; i < dr.FieldCount; i++) {
                            string name = dr.GetName(i);

                            fs.Write(name + ";");
                        }

                        fs.WriteLine();

                        while (dr.Read()) {
                            for (int i = 0; i < dr.FieldCount; i++) {
                                string value = dr[i].ToString();

                                fs.Write(value + ";");
                            }
                            fs.WriteLine();
                        }

                    } catch (Exception e) {
                        Console.WriteLine("Unable to read data from SQL DB: " + tableName, e);
                    }

                }


            }


        }

    }
}
