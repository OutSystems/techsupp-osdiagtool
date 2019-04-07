using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.OracleClient;

namespace OSDiagTool
{
    class DBConnection
    {
        // SQL Server DB Connection
        public static string SQLDBConnection(string dataSource, string initialCatalog, string userId, string pwd)
        {
            string connectionStringSQL = String.Format("Data Source={0};Initial Catalog={1};User id={2};Password={3};", dataSource, initialCatalog, userId, pwd);

            return connectionStringSQL;
        }

        // SQL Reader
        public void SQLReader(string dataSource, string initialCatalog, string userId, string pwd, string queryString)
        {
            string connectionString = SQLDBConnection(dataSource, initialCatalog, userId, pwd);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                try
                {
                    connection.Open();
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
                    Console.WriteLine("Unable to retrieve SQL DB information" + e.Message);
                }
            }
        }


        // Oracle DB Connection
        public static string OracleDBConnection(string host, string port, string serviceName, string userId, string pwd)
        {
            string connectionStringORCL = String.Format("SERVER=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})));uid = {3}; pwd = {4};", host, port, serviceName, userId, pwd);

            return connectionStringORCL;
        }

        // Oracle Reader
        public void OracleReader(string host, string port, string serviceName, string userId, string pwd, string queryString)
        {
            string connectionString = OracleDBConnection(host, port, serviceName, userId, pwd);

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand command = new OracleCommand(queryString, connection);

                try
                {
                    connection.Open();
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
                    Console.WriteLine("Unable to retrieve Oracle DB Information" + e.Message);
                }
            }
        }
      
    }
}
