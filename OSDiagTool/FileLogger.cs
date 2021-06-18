using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime;
using System.Threading;

namespace OSDiagTool
{
    public class FileLogger
    {
        private static string _tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "collect_data");
        private static string _consoleLog = Path.Combine(_tempFolderPath, "ConsoleLog.txt");


        public static void LogError(string customMessage, string errorMessage, bool writeToConsole = true, bool writeDateTime = true)
        {
            if (writeToConsole) { Console.WriteLine("[ERROR] " + customMessage + ": " + errorMessage);  };
            File.AppendAllText(_consoleLog, writeDateTime ?  DateTime.Now + "\t" + "[ERROR] \t" + customMessage + " [" + Thread.CurrentThread.ManagedThreadId + "]" + "\t" + errorMessage + Environment.NewLine : 
                "[" + Thread.CurrentThread.ManagedThreadId + "]" + "[ERROR] \t" + customMessage + "\t" + errorMessage + Environment.NewLine);

        }

        public static void TraceLog(string traceMessage, bool isTaskFinished = false, bool writeDateTime = true)
        {
            if (isTaskFinished == false)
            {
                Console.Write(traceMessage);
                File.AppendAllText(_consoleLog, writeDateTime ? Environment.NewLine + DateTime.Now + " [" + Thread.CurrentThread.ManagedThreadId + "]" + "\t" + traceMessage  : Environment.NewLine + "[" + Thread.CurrentThread.ManagedThreadId + "]" + traceMessage );
            }
            else if (isTaskFinished == true)
            {
                Console.WriteLine(traceMessage);
                File.AppendAllText(_consoleLog, traceMessage);
            }

        }

    }
}
