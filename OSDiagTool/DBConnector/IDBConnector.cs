using System;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;

namespace OSDiagTool
{
    interface ISQLDBConnector
    {
        SqlConnection SQLOpenConnection(DBConnector.SQLConnStringModel SQLConnectionString);

    }

    interface IOracleDBConnector
    {
        OracleConnection OracleOpenConnection (DBConnector.OracleConnStringModel OracleConnectionString);
    }

    public interface IDatabaseConnection : IDisposable
    {
        void Connect(DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null);
        OracleConnection ReturnOracleConnection();
        SqlConnection ReturnSQLConnection();
    }
}
