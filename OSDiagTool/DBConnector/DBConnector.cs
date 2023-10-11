using System;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;


namespace OSDiagTool.DBConnector
{
    class SLQDBConnector : ISQLDBConnector
    {
        // SQL Open Connection       
        public SqlConnection SQLOpenConnection(DBConnector.SQLConnStringModel SQLConnectionString)
        {
            string connectionString = SQLConnString(SQLConnectionString);

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

        // SQL Server DB Connection String
        public string SQLConnString(DBConnector.SQLConnStringModel SQLConnectionString)
        {
            string connectionStringSQL = String.Format("Data Source={0};Initial Catalog={1};User id={2};Password={3}; MultipleActiveResultSets=true;",
                SQLConnectionString.dataSource, SQLConnectionString.initialCatalog, SQLConnectionString.userId, SQLConnectionString.pwd);

            return connectionStringSQL;
        }

        // SQL Close Connection
        public void SQLCloseConnection(SqlConnection connection)
        {
            connection.Close();
        }

    }


    class OracleDBConnector : IOracleDBConnector
    {

        public OracleConnection OracleOpenConnection(DBConnector.OracleConnStringModel OracleConnectionString)
        {
            string connectionString = OracleConnString(OracleConnectionString);

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

        // Oracle DB Connection String
        public string OracleConnString(DBConnector.OracleConnStringModel OracleConnectionString)
        {
            string connectionStringORCL = String.Format("Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2}))); User Id = {3}; Password = {4};",
                OracleConnectionString.host, OracleConnectionString.port, OracleConnectionString.serviceName, OracleConnectionString.userId, OracleConnectionString.pwd);

            return connectionStringORCL;

        }

        // Oracle Close Connection
        public void OracleCloseConnection(OracleConnection connection)
        {
            connection.Close();
        }
    }
}
