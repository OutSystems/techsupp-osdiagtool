using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.OSDiagToolConf {
    class OSDiagToolHelper {

        public static int CountSteps(Dictionary<string, bool> configurations) {

            int steps = 0;

            foreach (KeyValuePair<string, bool> conf in configurations) {

                if (conf.Value.Equals(true)) {
                    steps++;
                }
            }

            return steps;

        }




    }
}
