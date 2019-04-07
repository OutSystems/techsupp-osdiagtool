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
        // SQL Server DB Connection String
        public static string SQLConnString(string dataSource, string initialCatalog, string userId, string pwd)
        {
            string connectionStringSQL = String.Format("Data Source={0};Initial Catalog={1};User id={2};Password={3};", dataSource, initialCatalog, userId, pwd);

            return connectionStringSQL;
        }

        // SQL Open Connection
        public static SqlConnection SQLOpenConnection(string dataSource, string initialCatalog, string userId, string pwd)
        {
            string connectionString = SQLConnString(dataSource, initialCatalog, userId, pwd);

            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                SQLCloseConnection(connection);
                Console.WriteLine("Unable to open SQL DB connection" + e.Message);
                throw e;
            }
        }

        // SQL Reader
        public static void SQLReader(string dataSource, string initialCatalog, string userId, string pwd, string queryString)
        {
            SqlConnection connection = SQLOpenConnection(dataSource, initialCatalog, userId, pwd);

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
                    SQLCloseConnection(connection);
                }
            }
        }

        // SQL Close Connection
        public static void SQLCloseConnection(SqlConnection connection)
        {
            connection.Close();
        }


        // Oracle DB Connection String
        public static string OracleConnString(string host, string port, string serviceName, string userId, string pwd)
        {
            string connectionStringORCL = String.Format("SERVER=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})));uid = {3}; pwd = {4};", host, port, serviceName, userId, pwd);

            return connectionStringORCL;
        }

        // Oracle Open Connection
        public static OracleConnection OracleOpenConnection(string host, string port, string serviceName, string userId, string pwd)
        {
            string connectionString = OracleConnString(host, port, serviceName, userId, pwd);

            OracleConnection connection = new OracleConnection(connectionString);

            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                OracleCloseConnection(connection);
                Console.WriteLine("Unable to retrieve Oracle DB Information" + e.Message);
                throw e;
            }
        }

        // Oracle Reader
        public static void OracleReader(string host, string port, string serviceName, string userId, string pwd, string queryString)
        {
            OracleConnection connection = OracleOpenConnection(host, port, serviceName, userId, pwd);

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
                    OracleCloseConnection(connection);
                }
            }
        }

        // Oracle Close Connection
        public static void OracleCloseConnection(OracleConnection connection)
        {
            connection.Close();
        }

    }
}
