using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace OSDiagTool.OSDiagTool {
    class OSDiagToolConfReader {

        private static string _osDGTConfFile = "OSDiagTool.exe.config";
        private static string _osDiagToolConfigPath = Path.Combine(Directory.GetCurrentDirectory(), _osDGTConfFile);
        // XML Elements
        // Root
        private static string _rootElement = "configuration";

        //  L1
        private static string _l1_infoToRetrieve = "infoToRetrieve";
        private static string _l1_iisConf = "IisConf";
        private static string _l1_osDiagToolConf = "OSDiagToolConf";

        // L2
        private static string _l2_databaseTables = "databaseTables";
        private static string _l2_iisLogsNrDays = "LogsNumberOfDays";
        private static string _l2_queryTimeout = "queryTimeout";

        // L3
        private static string _l3_ossys = "ossys";
        private static string _l3_osltm = "osltm";

        // XML Attributes
        private static string _nameAttribute = "name";


        public List<string> GetTableNames(bool isLifeTimeEnv) {

            List<string> tableNames = new List<string> ();

            XDocument xml = XDocument.Load(_osDiagToolConfigPath);

            // Reading OSSYS table names
            var ossysNodes = (from n in xml.Descendants(_rootElement)
                         select n.Element(_l1_infoToRetrieve).Element(_l2_databaseTables).Element(_l3_ossys).Elements()).ToList();

            // Read OSLTM tables if it's LT environment
            if (isLifeTimeEnv) {
                var osltmNodes = (from n in xml.Descendants(_rootElement)
                                  select n.Element(_l1_infoToRetrieve).Element(_l2_databaseTables).Element(_l3_osltm).Elements()).ToList();

                foreach (XElement el in osltmNodes[0]) {

                    string tableName = el.Attribute(_nameAttribute).Value;
                    tableNames.Add(tableName);

                }
            }

            foreach (XElement el in ossysNodes[0]) {

                string tableName = el.Attribute(_nameAttribute).Value;
                tableNames.Add(tableName);

            }

            return tableNames;

        }

        public int GetIISLogsNrDays() {
            
            XDocument xml = XDocument.Load(_osDiagToolConfigPath);

            var query = from n in xml.Descendants(_rootElement)
                        select new {
                            daysOfLogs = n.Element(_l1_iisConf).Element(_l2_iisLogsNrDays).Value,
                        };

            int iisLogsNrDays = Convert.ToInt32(query.First().daysOfLogs);
            return iisLogsNrDays;

        }

        public int GetQueryTimeout() {

            XDocument xml = XDocument.Load(_osDiagToolConfigPath);

            var query = from n in xml.Descendants(_rootElement)
                        select new {
                            qTimeout = n.Element(_l1_osDiagToolConf).Element(_l2_queryTimeout).Value,
                        };

            int queryTimeout = Convert.ToInt32(query.First().qTimeout);

            return queryTimeout;
        }
    }
}
