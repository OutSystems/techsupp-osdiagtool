using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;

namespace OSDiagTool.DBConnector
{
    public interface IDatabaseCommand
    {
        void Execute(string query, OSDiagToolConf.ConfModel.strConfModel configurations, SqlConnection sqlConnection = null, OracleConnection oracleConnection = null);
        IEnumerable<object[]> ReadData(string query, OSDiagToolConf.ConfModel.strConfModel configurations, IDatabaseConnection connection);
    }
}
