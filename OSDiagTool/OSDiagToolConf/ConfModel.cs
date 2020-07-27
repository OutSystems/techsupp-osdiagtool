using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.OSDiagToolConf {
    public class ConfModel {

        public class strConfModel{

            public int queryTimeout { get; set; }
            public int IISLogsNrDays { get; set; }

            public int osLogTopRecords { get; set; }

            public List<string> tableNames { get; set; }

            public Dictionary<string, Dictionary<string, bool>> osDiagToolConfigurations { get; set; }

            public Dictionary<string, Dictionary<string, string>> databaseQueryConfigurations { get; set; }

        }

    }
}
