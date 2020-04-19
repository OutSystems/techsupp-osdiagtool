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
        private static string _l1_infoToRetrieve = "databaseInfo";
        private static string _l1_iisConf = "IisConf";
        private static string _l1_osDiagToolConf = "OSDiagToolConf";

        // L2
        private static string _l2_databaseTables = "databaseTables";
        private static string _l2_iisLogsNrDays = "LogsNumberOfDays";
        private static string _l2_queryTimeout = "queryTimeout";
        public static string _l2_threadDumps = "threadDumps";
        public static string _l2_memoryDumps = "memoryDumps";
        public static string _l2_serverLogs = "serverLogs";
        public static string _l2_databaseOperations = "databaseOperations";
        public static string _l2_platform = "platform";
        private static string _l2_databaseQueryConfigurations = "databaseQueryConfigurations";

        // L3
        private static string _l3_ossys = "ossys";
        private static string _l3_osltm = "osltm";
        private static string _l3_oslog = "oslog";
        public static string _l3_iisW3wp = "iisW3wp";
        public static string _l3_osServices = "osServices";
        public static string _l3_evtAndServer = "evtAndServer";
        public static string _l3_iisLogs = "iisLogs";
        public static string _l3_platformMetamodel = "platformMetamodel";
        public static string _l3_databaseTroubleshoot = "databaseTroubleshoot";
        public static string _l3_platformLogs = "platformLogs";
        public static string _l3_sqlServer = "sqlServer";
        public static string _l3_oracle = "oracle";
        public static string _l3_oslogTopRecords = "oslogTopRecords";
        public static string _l3_platformAndServerConfigFiles = "platformAndServerConfFiles";

        // L4
        public static string _l4_top_statCachedPlans = "top_statCachedPlans";
        public static string _l4_top_topCPU = "top_topCPU";

        // XML Attributes
        private static string _nameAttribute = "name";


        public OSDiagToolConf.ConfModel.strConfModel GetOsDiagToolConfigurations() {

            XDocument xml = XDocument.Load(_osDiagToolConfigPath);

            var configuration = new OSDiagToolConf.ConfModel.strConfModel();

            configuration.queryTimeout = GetQueryTimeout(xml);
            configuration.IISLogsNrDays = GetIISLogsNrDays(xml);
            configuration.tableNames = GetTableNames(xml);
            configuration.osLogTopRecords = GetOsLogTopRecords(xml);
            configuration.databaseQueryConfigurations = GetDatabaseQueryConfigurations(xml);
            configuration.osDiagToolConfigurations = GetOsDiagToolConfigurations(xml);

            return configuration;

        }
        
        public List<string> GetTableNames(XDocument xml) {

            List<string> tableNames = new List<string> ();

            // Read OSLOG tables
            var oslogNodes = (from n in xml.Descendants(_rootElement)
                              select n.Element(_l1_infoToRetrieve).Element(_l2_databaseTables).Element(_l3_oslog).Elements()).ToList();

            // Reading OSSYS table names
            var ossysNodes = (from n in xml.Descendants(_rootElement)
                         select n.Element(_l1_infoToRetrieve).Element(_l2_databaseTables).Element(_l3_ossys).Elements()).ToList();

            // Read OSLTM tables if it's LT environment
            
            var osltmNodes = (from n in xml.Descendants(_rootElement)
                                select n.Element(_l1_infoToRetrieve).Element(_l2_databaseTables).Element(_l3_osltm).Elements()).ToList();

            foreach (XElement el in osltmNodes[0]) {

                string tableName = el.Attribute(_nameAttribute).Value;
                Regex pattern = new Regex("[ -*/()';]|[\n]{2}/g");
                tableName = pattern.Replace(tableName, "");

                // Check if table name in configuration file matchs prefix of table
                if (tableName.ToLower().StartsWith(_l3_osltm) && !(tableName.ToLower().Contains((" ")))) {
                    tableNames.Add(tableName);
                                       
                }
            }

            foreach (XElement el in ossysNodes[0]) {

                string tableName = el.Attribute(_nameAttribute).Value;
                Regex pattern = new Regex("[ -*/]|[\n]{2}/g");
                tableName = pattern.Replace(tableName, "");

                // Check if table name in configuration file matchs prefix of table and delete everything after space and comma to protect from SQLI
                if (tableName.ToLower().StartsWith(_l3_ossys) && !(tableName.ToLower().Contains((" ")))) {
                    tableNames.Add(tableName);
                }
                
            }

            foreach (XElement el in oslogNodes[0]) {

                string tableName = el.Attribute(_nameAttribute).Value;
                Regex pattern = new Regex("[ -*/]|[\n]{2}/g");
                tableName = pattern.Replace(tableName, "");

                // Check if table name in configuration file matchs prefix of table and delete everything after space and comma to protect from SQLI
                if (tableName.ToLower().StartsWith(_l3_oslog) && !(tableName.ToLower().Contains((" ")))) {
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

        public int GetOsLogTopRecords (XDocument xml) {

            var query = from n in xml.Descendants(_rootElement)
                        select new {
                            value = n.Element(_l1_infoToRetrieve).Element(_l2_databaseQueryConfigurations).Element(_l3_oslogTopRecords).Value,
                        };
            try {
                int osLogTopRecords = Convert.ToInt32(query.First().value);
                return osLogTopRecords;
            } catch (Exception e) {
                int osLogTopRecords = 500;
                return osLogTopRecords;
            }

        }

        public Dictionary<string, Dictionary<string, string>> GetDatabaseQueryConfigurations(XDocument xml) {

            Dictionary<string, Dictionary<string, string>> DatabaseQueryConfigurations = new Dictionary<string, Dictionary<string, string>>();

            var sqlServerNodesConf = (from n in xml.Descendants(_rootElement)
                              select n.Element(_l1_infoToRetrieve).Element(_l2_databaseQueryConfigurations).Element(_l3_sqlServer).Elements()).ToList();

            var oracleNodesConf = (from n in xml.Descendants(_rootElement)
                                      select n.Element(_l1_infoToRetrieve).Element(_l2_databaseQueryConfigurations).Element(_l3_oracle).Elements()).ToList();

            foreach (XElement el in sqlServerNodesConf[0]) {

                string conf = el.Value;
                XName XconfName = el.Name;
                string confName = XconfName.ToString();

                if (!DatabaseQueryConfigurations.ContainsKey(_l3_sqlServer)) {
                    DatabaseQueryConfigurations.Add(_l3_sqlServer, new Dictionary<string, string>());
                }

                DatabaseQueryConfigurations[_l3_sqlServer].Add(confName, conf);

            }

            foreach (XElement el in oracleNodesConf[0]) {

                string conf = el.Value;
                XName XconfName = el.Name;
                string confName = XconfName.ToString();

                if (!DatabaseQueryConfigurations.ContainsKey(_l3_oracle)) {
                    DatabaseQueryConfigurations.Add(_l3_oracle, new Dictionary<string, string>());
                }

                DatabaseQueryConfigurations[_l3_oracle].Add(confName, conf);

            }

            return DatabaseQueryConfigurations;

        }

        public Dictionary<string, Dictionary<string, bool>> GetOsDiagToolConfigurations(XDocument xml) {

            Dictionary<string, Dictionary<string, bool>> OsDiagToolConfigurations = new Dictionary<string, Dictionary<string, bool>>();

            var threadDumpsNodes = (from n in xml.Descendants(_rootElement)
                                      select n.Element(_l1_osDiagToolConf).Element(_l2_threadDumps).Elements()).ToList();

            var memoryDumpsNodes = (from n in xml.Descendants(_rootElement)
                                    select n.Element(_l1_osDiagToolConf).Element(_l2_memoryDumps).Elements()).ToList();

            var serverLogsNodes = (from n in xml.Descendants(_rootElement)
                                    select n.Element(_l1_osDiagToolConf).Element(_l2_serverLogs).Elements()).ToList();

            var platformNodes = (from n in xml.Descendants(_rootElement)
                                   select n.Element(_l1_osDiagToolConf).Element(_l2_platform).Elements()).ToList();

            var databaseOperationsNodes = (from n in xml.Descendants(_rootElement)
                                   select n.Element(_l1_osDiagToolConf).Element(_l2_databaseOperations).Elements()).ToList();

            foreach (XElement el in threadDumpsNodes[0]) {

                bool conf = Convert.ToBoolean(el.Value);
                XName XconfName = el.Name;
                string confName = XconfName.ToString();

                if (!OsDiagToolConfigurations.ContainsKey(_l2_threadDumps)) {
                    OsDiagToolConfigurations.Add(_l2_threadDumps, new Dictionary<string, bool>());
                }

                OsDiagToolConfigurations[_l2_threadDumps].Add(confName, conf);

            }

            foreach (XElement el in memoryDumpsNodes[0]) {

                bool conf = Convert.ToBoolean(el.Value);
                XName XconfName = el.Name;
                string confName = XconfName.ToString();

                if (!OsDiagToolConfigurations.ContainsKey(_l2_memoryDumps)) {
                    OsDiagToolConfigurations.Add(_l2_memoryDumps, new Dictionary<string, bool>());
                }
                
                OsDiagToolConfigurations[_l2_memoryDumps].Add(confName, conf);

            }

            foreach (XElement el in serverLogsNodes[0]) {

                bool conf = Convert.ToBoolean(el.Value);
                XName XconfName = el.Name;
                string confName = XconfName.ToString();

                if (!OsDiagToolConfigurations.ContainsKey(_l2_serverLogs)) {
                    OsDiagToolConfigurations.Add(_l2_serverLogs, new Dictionary<string, bool>());
                }

                OsDiagToolConfigurations[_l2_serverLogs].Add(confName, conf);

            }

            foreach (XElement el in platformNodes[0]) {

                bool conf = Convert.ToBoolean(el.Value);
                XName XconfName = el.Name;
                string confName = XconfName.ToString();

                if (!OsDiagToolConfigurations.ContainsKey(_l2_platform)) {
                    OsDiagToolConfigurations.Add(_l2_platform, new Dictionary<string, bool>());
                }

                OsDiagToolConfigurations[_l2_platform].Add(confName, conf);

            }

            foreach (XElement el in databaseOperationsNodes[0]) {

                bool conf = Convert.ToBoolean(el.Value);
                XName XconfName = el.Name;
                string confName = XconfName.ToString();

                if (!OsDiagToolConfigurations.ContainsKey(_l2_databaseOperations)) {
                    OsDiagToolConfigurations.Add(_l2_databaseOperations, new Dictionary<string, bool>());
                }

                OsDiagToolConfigurations[_l2_databaseOperations].Add(confName, conf);

            }

            return OsDiagToolConfigurations;

        }
    }
}
