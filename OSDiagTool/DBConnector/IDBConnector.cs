using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
