using System.Collections.Generic;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;

namespace OSDiagTool.DBConnector
{
    public interface IDatabaseCommand
    {
        void Execute(string query, OSDiagToolConf.ConfModel.strConfModel configurations, SqlConnection sqlConnection = null, OracleConnection oracleConnection = null);
        IEnumerable<object[]> ReadData(string query, OSDiagToolConf.ConfModel.strConfModel configurations, IDatabaseConnection connection, string oracleAdminSchema = null);
    }
}
