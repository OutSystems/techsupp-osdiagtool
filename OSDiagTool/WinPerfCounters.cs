using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace OSDiagTool {
    class WinPerfCounters {

        private static string _aspNetCategory = "ASP.NET";
        private static string _iisQueueCounter = "Requests Queued";

        public static float GetCPUUsage() {

            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            return cpuCounter.NextValue();

        }

        public static float GetIISQueue() {

            float iisQueue = 0;

            PerformanceCounter perfCounter = new PerformanceCounter(_aspNetCategory, _iisQueueCounter, true);

            using (perfCounter) {
                iisQueue = perfCounter.NextValue();
            }

            return iisQueue;

        }

        public static bool IISQueueAlarm(float queueThreshold) {

            bool alarm = false;

            float iisQueue = WinPerfCounters.GetIISQueue();

            if (iisQueue >= queueThreshold) {

                return alarm = true;

            }

            return alarm;

        }

        public static int GetParentProcess(int childProcessId) {

            var parentPrc = new PerformanceCounter("Process", "Creating Process ID", FindIndexedProcessName(childProcessId), true);
            Process pid = Process.GetProcessById((int)parentPrc.NextValue());

            return Convert.ToInt32(pid.Id);

        }

        private static string FindIndexedProcessName(int pid) {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++) {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                if ((int)processId.NextValue() == pid) {
                    return processIndexdName;
                }
            }

            return processIndexdName;
        }

    }
}
