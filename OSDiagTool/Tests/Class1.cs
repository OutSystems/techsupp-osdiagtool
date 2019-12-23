using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDiagTool.DBConnector;
using OSDiagTool.Platform.ConfigFiles;

namespace OSDiagTool.Tests
{
    class Class1
    {
        static void Main(string[] args)
        {
            var connString = new DBConnector.SQLConnStringModel();
            connString.dataSource = "<Hostname>";
            connString.initialCatalog = "<Catalog>";
            connString.userId = "<user>";
            connString.pwd = "<pwd>";

            OSDiagTool.DBConnector.DBReader.SQLReader(connString, "SELECT TOP 5 * FROM OSSYS_ESPACE");

        }
    }
}
