using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.OracleClient;

namespace OSDiagTool.DBConnector
{
    class DBReader
    {
        // SQL Reader
        public static void SQLReader(OSDiagTool.DBConnector.SQLConnStringModel SQLConnectionString, string queryString)
        {
            var connector = new DBConnector.SLQDBConnector();
            SqlConnection connection = connector.SQLOpenConnection(SQLConnectionString);

            using (connection)
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // TODO: Implementation of query export to csv file in batches
                        Console.WriteLine("\t{0}\t{1}\t{2}\t{3}\t{4}", reader[0], reader[1], reader[2], reader[3], reader[4]);
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to read data from SQL DB: ", e);
                }
                finally
                {
                    connector.SQLCloseConnection(connection);
                }
            }
        }


        // Oracle Reader
        public static void OracleReader(DBConnector.OracleConnStringModel OracleConnectionString, string queryString)
        {
            var connector = new DBConnector.OracleDBConnector();
            OracleConnection connection = connector.OracleOpenConnection(OracleConnectionString);

            using (connection)
            {
                OracleCommand command = new OracleCommand(queryString, connection);

                try
                {
                    OracleDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // TODO: Implementation of query export to csv file in batches
                        Console.WriteLine("\t{0}\t{1}\t{2}\t{3}\t{4}", reader[0], reader[1], reader[2], reader[3], reader[4]);
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to read data from Oracle DB: ", e);
                }
                finally
                {
                    connector.OracleCloseConnection(connection);
                }
            }
        }
    }
}
