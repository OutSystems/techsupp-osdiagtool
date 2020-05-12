using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.OSDiagToolForm {
    class DataHelperClass {

        public class strConfigurations {

            public OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations { get; set; }

            public OSDiagToolConf.ConfModel.strConfModel ConfigFileConfigurations { get; set; }

            public puf_popUpForm popup { get; set; }

        }
    }
}
