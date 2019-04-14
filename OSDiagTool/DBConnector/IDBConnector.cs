using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.OracleClient;

namespace OSDiagTool
{
    interface ISQLDBConnector
    {

        string dataSource { get; set; }

        string initialCatalog { get; set; }

        string userId { get; set; }

        string pwd { get; set; }

        SqlConnection SQLOpenConnection(DBConnector.SQLConnStringModel SQLConnectionString);

    }

    interface IOracleDBConnector
    {

        string host { get; set; }

        string port { get; set; }

        string serviceName { get; set; }

        string userId { get; set; }

        string pwd { get; set; }

        OracleConnection OracleOpenConnection(DBConnector.OracleConnStringModel OracleConnectionString);

    }
}
