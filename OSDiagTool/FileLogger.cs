using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime;

namespace OSDiagTool
{
    public class FileLogger
    {
        private static string _tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "collect_data");
        private static string _errorDumpFile = Path.Combine(_tempFolderPath, "ConsoleLog.txt");


        public static void LogError(string customMessage, string errorMessage)
        {
            Console.WriteLine("[ERROR] " + customMessage + ": " + errorMessage);
            File.AppendAllText(_errorDumpFile, DateTime.Now + "\t" + "[ERROR] \t" + customMessage + "\t" + errorMessage + Environment.NewLine);
        }

        public static void TraceLog(string traceMessage, bool isTaskFinished = false)
        {
            if (isTaskFinished == false)
            {
                Console.Write(traceMessage);
                File.AppendAllText(_errorDumpFile, DateTime.Now + "\t" + traceMessage);
            }
            else if (isTaskFinished == true)
            {
                Console.WriteLine(traceMessage);
                File.AppendAllText(_errorDumpFile, traceMessage + Environment.NewLine);
            }

        }

    }
}
