using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OSDiagTool.DBConnector
{
    public class SqlCommandExecutor : IDatabaseCommand
    {
        private IDatabaseConnection connection;
        private SqlConnection sqlConnection;

        public SqlCommandExecutor(IDatabaseConnection sqlConnection)
        {
            connection = sqlConnection;
        }

        public void Execute(string query, OSDiagToolConf.ConfModel.strConfModel configurations, SqlConnection sqlConnection, OracleConnection oracleConnection = null)
        {
            SqlCommand command = new SqlCommand(query, sqlConnection)
            {
                CommandTimeout = configurations.queryTimeout
            };
        }

        public IEnumerable<object[]> ReadData(string query, OSDiagToolConf.ConfModel.strConfModel configurations, IDatabaseConnection connection, string oracleAdminSchema = null)
        {
            List<object[]> result = new List<object[]>();
            if (connection is SqlConnector)
            {
                sqlConnection = connection.ReturnSQLConnection();
            }
            SqlCommand command = new SqlCommand(query, sqlConnection)
            {
                CommandTimeout = configurations.queryTimeout
            };

            using (SqlDataReader reader = command.ExecuteReader())
         {
             while (reader.Read())
             {
                 object[] rowData = new object[reader.FieldCount];
                 reader.GetValues(rowData);
                 result.Add(rowData);
             }
         }
            return result;
        }
    }

    public class OracleCommandExecutor : IDatabaseCommand
    {
        private IDatabaseConnection connection;
        private OracleConnection oracleConnection;

        public OracleCommandExecutor(IDatabaseConnection oracleConnection)
        {
            connection = oracleConnection;
        }

        public void Execute(string query, OSDiagToolConf.ConfModel.strConfModel configurations, SqlConnection sqlConnection = null, OracleConnection oracleConnection = null)
        {
            if (oracleConnection == null || oracleConnection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Oracle connection not initialized or closed.");
            }

            using (OracleCommand command = new OracleCommand(query, oracleConnection))
            {
                command.CommandTimeout = configurations.queryTimeout;
            }
        }

        public IEnumerable<object[]> ReadData(string query, OSDiagToolConf.ConfModel.strConfModel configurations, IDatabaseConnection connection, string oracleAdminSchema = null)
        {
            List<object[]> result = new List<object[]>();
            if(connection is OracleConnector)
            {
                oracleConnection = connection.ReturnOracleConnection();
            }

            string alterSessionSql = String.Format("ALTER SESSION SET CURRENT_SCHEMA = {0}", oracleAdminSchema);
            
            using (OracleTransaction transaction = oracleConnection.BeginTransaction())
            {
                try
                {
                    using (OracleCommand alterSessionCmd = new OracleCommand(alterSessionSql, oracleConnection))
                    {
                        alterSessionCmd.Transaction = transaction;
                        alterSessionCmd.ExecuteNonQuery();
                    }

                    OracleCommand command = new OracleCommand(query, oracleConnection)
                    {
                        CommandTimeout = configurations.queryTimeout
                    };

                    command.Transaction = transaction;

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] rowData = new object[reader.FieldCount];
                            reader.GetValues(rowData);
                            result.Add(rowData);
                        }
                    }

                    transaction.Commit();

                    return result;

                } catch (Exception e)
                {
                    FileLogger.LogError("Eror executing Oracle reader.", e.Message + e.StackTrace);
                    transaction.Rollback();
                    return null;
                }
            }
        }
    }

    public static class DatabaseCommandFactory
    {
        public static IDatabaseCommand GetCommandExecutor(Database.DatabaseType dbEngine, IDatabaseConnection connection)
        {
            switch (dbEngine)
            {
                case Database.DatabaseType.SqlServer:
                    return new SqlCommandExecutor(connection);
                case Database.DatabaseType.Oracle:
                    return new OracleCommandExecutor(connection);
                default:
                    throw new ArgumentException("Unsupported database type while getting command executor");
            }
        }
    }
}
