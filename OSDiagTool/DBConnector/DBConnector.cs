using System;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;


namespace OSDiagTool.DBConnector
{
    public class SLQDBConnector : ISQLDBConnector
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


//
    public class SqlConnector : IDatabaseConnection, IDisposable
    {
        public SqlConnection sqlConnection;

        public string SQLConnString(DBConnector.SQLConnStringModel SQLConnectionString)
        {
            string connectionStringSQL = String.Format("Data Source={0};Initial Catalog={1};User id={2};Password={3}; MultipleActiveResultSets=true;",
                SQLConnectionString.dataSource, SQLConnectionString.initialCatalog, SQLConnectionString.userId, SQLConnectionString.pwd);

            return connectionStringSQL;
        }
        public void Connect(DBConnector.SQLConnStringModel SQLConnectionString, DBConnector.OracleConnStringModel OracleConnectionString = null)
        {
            string connectionString = SQLConnString(SQLConnectionString);

            sqlConnection = new SqlConnection(connectionString);

            try
            {
                sqlConnection.Open();
            }
            catch (Exception e)
            {
                sqlConnection.Close();
                Console.WriteLine("Unable to open SQL DB connection" + e.Message);
                throw e;
            }
        }

        public void Dispose()
        {
            if (sqlConnection != null)
            {
                sqlConnection.Close();
            }
        }

        public SqlConnection ReturnSQLConnection()
        {
            if (sqlConnection != null && sqlConnection.State == System.Data.ConnectionState.Open)
            {
                return sqlConnection;
            }
            else
            {
                throw new InvalidOperationException("SQL connection is not open or initialized.");
            }
        }

        public OracleConnection ReturnOracleConnection()
        {
            throw new NotSupportedException(); // not supported in SQL Connector - do not use
        }

    }

    public class OracleConnector : IDatabaseConnection, IDisposable
    {
        private OracleConnection oracleConnection;

        public string OracleConnString(DBConnector.OracleConnStringModel OracleConnectionString)
        {
            string connectionStringORCL = String.Format("Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2}))); User Id = {3}; Password = {4};",
                OracleConnectionString.host, OracleConnectionString.port, OracleConnectionString.serviceName, OracleConnectionString.userId, OracleConnectionString.pwd);

            return connectionStringORCL;

        }
        public void Connect (DBConnector.SQLConnStringModel SQLConnectionString= null, DBConnector.OracleConnStringModel OracleConnectionString = null)
        {
            string connectionString = OracleConnString(OracleConnectionString);

            OracleConnection connection = new OracleConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                connection.Close();
                Console.WriteLine("Unable to retrieve Oracle DB Information" + e.Message);
                throw e;
            }
        }

        public void Dispose()
        {
            if(oracleConnection != null)
            {
                oracleConnection.Close();
            }
        }

        public OracleConnection ReturnOracleConnection()
        {
            if (oracleConnection != null && oracleConnection.State == System.Data.ConnectionState.Open)
            {
                return oracleConnection;
            }
            else
            {
                throw new InvalidOperationException("Oracle connection is not open or initialized.");
            }
        }

        public SqlConnection ReturnSQLConnection()
        {
            throw new NotSupportedException(); // not supported in Oracle Connector - do not use
        }
    }

    public static class DatabaseConnectionFactory
    {
        public static IDatabaseConnection GetDatabaseConnection(Database.DatabaseType dbEngine, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null)
        {
            switch (dbEngine)
            {
                case Database.DatabaseType.SqlServer:
                    SqlConnector sqlConnection = new SqlConnector();
                    sqlConnection.Connect(SQLConnectionString: SQLConnectionString);
                    return sqlConnection;

                case Database.DatabaseType.Oracle:
                    OracleConnector oracleConnection = new OracleConnector();
                    oracleConnection.Connect(OracleConnectionString: OracleConnectionString);
                    return oracleConnection;

                default:
                    throw new ArgumentException("Unsupported database engine");
            }
        }
    }

}
