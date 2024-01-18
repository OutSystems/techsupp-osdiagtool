using System.Collections.Generic;

namespace OSDiagTool.OSDiagToolForm {
    public class OsDiagFormConfModel {

        public class strFormConfigurationsModel {

            public Dictionary<string, bool> cbConfs { get; set; }

            public string saUser { get; set; }

            public string saPwd { get; set; }

            public int iisLogsNrDays { get; set; }

            public int osLogTopRecords { get; set; }

            public List<string> metamodelTables { get; set; }

        }

    }
}
