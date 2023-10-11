using System.Collections.Generic;

namespace OSDiagTool.OSDiagToolConf {
    public class ConfModel {

        public class strConfModel{

            public bool useMultiThread { get; set; }
            public int queryTimeout { get; set; }
            public int IISLogsNrDays { get; set; }

            public int osLogTopRecords { get; set; }

            public List<string> tableNames { get; set; }

            public Dictionary<string, Dictionary<string, bool>> osDiagToolConfigurations { get; set; }

            public Dictionary<string, Dictionary<string, string>> databaseQueryConfigurations { get; set; }

        }

    }
}
