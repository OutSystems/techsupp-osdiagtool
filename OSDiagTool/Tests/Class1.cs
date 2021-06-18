﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDiagTool.DBConnector;
using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.Utils;
using System.Xml.Linq;
using OSDiagTool.OSDiagToolConf;
using OSDiagTool.DatabaseExporter;
using System.Data.SqlClient;
using OSDiagTool.Database.DatabaseQueries;
using OSDiagTool;


namespace OSDiagTool.Tests
{
    class Class1
    {
        public static string PlatformDatabaseConfigurationElement = "PlatformDatabaseConfiguration";
        public static string _oSDiagToolConfPath = @"C:\Users\fcb\Desktop\GitProj\techsupp-osdiagtool\OSDiagTool\OSDiagTool.exe.config";
        //private static string platformConfigurationFilepath = @"C:\Users\fcb\Desktop\GitProj\techsupp-osdiagtool\OSDiagTool\bin\Debug\server.hsconf";
        //private static string _tempPath = @"C:\Users\fcb\Desktop\GitProj\techsupp-osdiagtool\OSDiagTool\bin\Debug";


        static void Main(string[] args)
        {

            Program.CollectMemoryDumps(true, false);


            //float test = WinPerfCounters.GetIISQueue();

            //Program.CollectMemoryDumps(true, true);

            /*string dbEngine = "oracle";
            int queryTimeout = 30;

            OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            var configurations = dgtConfReader.GetOsDiagToolConfigurations();

            var orclConnString = new DBConnector.OracleConnStringModel();
            orclConnString.host = @"<>";
            orclConnString.port = @"<>";
            orclConnString.serviceName = @"<>";

            orclConnString.userId = @"<>";
            orclConnString.pwd = @"<>";


            Database.DatabaseQueries.DatabaseTroubleshoot.DatabaseTroubleshooting(dbEngine, configurations, _tempPath, null, orclConnString);



            Test();

            //FileSystemHelper fs = new FileSystemHelper();
            //fs.DirectoryCopy(@"", @"", true, 0);

            var DQ = new OracleQueries();
            string foo = string.Format(DQ.alterSession, "test");

            //Database.DatabaseQueries.DatabaseTroubleshoot.DatabaseTroubleshooting("sqlserver", 30);
            
            
            
            
            
            var connString = new DBConnector.SQLConnStringModel();
            connString.dataSource = "";
            connString.initialCatalog = "";
            connString.userId = "";
            connString.pwd = "^";

            //DBReader.SQLReader(connString, "SELECT TOP 5 * FROM OSSYS_ESPACE");

            //ConfigFileReader confFileParser = new ConfigFileReader(platformConfigurationFilepath, "test");
            //ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;

            //string dbEngine = platformDBInfo.DBMS;

            var connector = new DBConnector.SLQDBConnector();
            SqlConnection connection = connector.SQLOpenConnection(connString);


            foreach(string table in configurations.tableNames) {
                //CSVExporter.SQLToCSVExport(connection, table, Path.Combine(Directory.GetCurrentDirectory(), "collect_data"), configurations.queryTimeout);
            }
            


            
            List<string> list1 = new List<string>()
            {
                "Server",
                "Catalog",
                "AdminUser",
                "AdminPassword"
            };


            HsconfReader(list1);

        
        }

        // TODO

        private static void OSDiagConfReader()
        {

            XDocument xml = XDocument.Load(_oSDiagToolConfPath);

            var nodes = (from n in xml.Descendants("configuration")
                            select n.Element("infoToRetrieve").Element("databaseTables").Element("ossys").Elements()).ToList();

            foreach (XElement el in nodes[0]) {

                string tableName = el.Attribute("name").Value;

            }


            //var xmlString = XDocument.Load(_oSDiagToolConfPath);


            // Querying the data
            var query = from p in xmlString.Descendants("configuration")
                        select new
                        {
                            test = p.Element("queryTimeout").Value
                            //test = p.Element("OSDiagToolConf").Attribute("queryTimeout").Value,
                        };

            string test = query.First().test.ToLower();


            */


            //return test; 
        }

        // must still be added to the Tool code
        private static void HsconfReader(List<string> HsconfPropertiesList)
        {
            //IDictionary<string,string> HsConfPropertiesVal;
            ConfigFileReader confFileParser = new ConfigFileReader("<ServerHsConfLocation>", "test");
            ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;
            string _propValue;

            IDictionary<string, string> HsConfPropertiesVal = new Dictionary<string, string>();

            foreach (string HsconfProperty in HsconfPropertiesList)
            {
                bool isEncrypted = platformDBInfo.GetProperty(HsconfProperty).IsEncrypted; // checking if prop is encrypted

                if (isEncrypted)
                {
                    _propValue = platformDBInfo.GetProperty(HsconfProperty).GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile("<ServerHsConfLocation>"));
                }
                else
                {
                    _propValue = platformDBInfo.GetProperty(HsconfProperty).Value;
                }

                HsConfPropertiesVal.Add(HsconfProperty, _propValue);
                Console.WriteLine(HsconfProperty + " " + _propValue);
            }
        }
    }
}
