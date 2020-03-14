using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace OSDiagTool.OSDiagToolConf {
    class OSDiagToolConfReader {

        private static string _osDGTConfFile = "OSDGTool.exe.config";
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


        public OSDiagToolConf.ConfModel.strConfModel GetOsDiagToolConfigurations() {

            XDocument xml = XDocument.Load(_osDiagToolConfigPath);

            var configuration = new OSDiagToolConf.ConfModel.strConfModel();

            configuration.queryTimeout = GetQueryTimeout(xml);
            configuration.IISLogsNrDays = GetIISLogsNrDays(xml);
            configuration.tableNames = GetTableNames(xml); 

            return configuration;

        }
        
        public List<string> GetTableNames(XDocument xml) {

            List<string> tableNames = new List<string> ();

            // Reading OSSYS table names
            var ossysNodes = (from n in xml.Descendants(_rootElement)
                         select n.Element(_l1_infoToRetrieve).Element(_l2_databaseTables).Element(_l3_ossys).Elements()).ToList();

            // Read OSLTM tables if it's LT environment
            
            var osltmNodes = (from n in xml.Descendants(_rootElement)
                                select n.Element(_l1_infoToRetrieve).Element(_l2_databaseTables).Element(_l3_osltm).Elements()).ToList();

            foreach (XElement el in osltmNodes[0]) {

                string tableName = el.Attribute(_nameAttribute).Value;
                Regex pattern = new Regex("[ -*/()]|[\n]{2}/g");
                pattern.Replace(tableName, "--");

                // Check if table name in configuration file matchs prefix of table
                if (tableName.ToLower().StartsWith(_l3_osltm)) {
                    tableNames.Add(tableName);
                                       
                }
            }

            foreach (XElement el in ossysNodes[0]) {

                string tableName = el.Attribute(_nameAttribute).Value;
                Regex pattern = new Regex("[ -*/]|[\n]{2}/g");
                pattern.Replace(tableName, "--");

                // Check if table name in configuration file matchs prefix of table and delete everything after space and comma to protect from SQLI
                if (tableName.ToLower().StartsWith(_l3_ossys)) {
                    tableNames.Add(tableName);
                }
                
            }

            return tableNames;

        }

        public int GetIISLogsNrDays(XDocument xml) {

            var query = from n in xml.Descendants(_rootElement)
                        select new {
                            daysOfLogs = n.Element(_l1_iisConf).Element(_l2_iisLogsNrDays).Value,
                        };
            try {
                int iisLogsNrDays = Convert.ToInt32(query.First().daysOfLogs);
                return iisLogsNrDays;
            } catch (Exception e){
                int iisLogsNrDays = 3;
                return iisLogsNrDays;
            }

        }

        public int GetQueryTimeout(XDocument xml) {

            var query = from n in xml.Descendants(_rootElement)
                        select new {
                            qTimeout = n.Element(_l1_osDiagToolConf).Element(_l2_queryTimeout).Value,
                        };
            try {
                int queryTimeout = Convert.ToInt32(query.First().qTimeout);
                return queryTimeout;
            } catch (Exception e) {
                int queryTimeout = 30;
                return queryTimeout;
            }

        }
    }
}
