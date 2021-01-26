using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OSDiagTool {
    class WinPerfCounters {

        private static string _aspNetCategory = "ASP.NET";
        private static string _iisQueueCounter = "Requests Queued";

        public static float GetIISQueue() {

            float iisQueue = 0;

            PerformanceCounter perfCounter = new PerformanceCounter(_aspNetCategory, _iisQueueCounter, true);

            using (perfCounter) {
                iisQueue = perfCounter.NextValue();
            }

            return iisQueue;

        }

    }
}
