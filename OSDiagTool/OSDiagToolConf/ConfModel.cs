using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.OSDiagToolConf {
    public class ConfModel {

        public struct strConfModel{

            public int queryTimeout { get; set; }
            public int IISLogsNrDays { get; set; }
            public List<string> tableNames { get; set; }

        }

    }
}
