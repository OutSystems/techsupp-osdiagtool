using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.OSDiagToolForm {
    class OsDiagFormConfModel {

        public struct strFormConfigurationsModel {

            public Dictionary<string, bool> cbConfs { get; set; }

            public string saUser { get; set; }

            public string saPwd { get; set; }

            public int iisLogsNrDays { get; set; }

            public int osLogTopRecords { get; set; }

            public List<string> metamodelTables { get; set; }

        }

    }
}
