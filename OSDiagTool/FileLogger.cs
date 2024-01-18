using System;
using System.IO;
using System.Threading;
using System.Reflection;

namespace OSDiagTool
{
    public class FileLogger
    {
        private static string _tempFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "collect_data");
        private static string _consoleLog = Path.Combine(_tempFolderPath, "ConsoleLog.txt");
        static ReaderWriterLock rwl = new ReaderWriterLock();

        public static void LogError(string customMessage, string errorMessage, bool writeToConsole = true, bool writeDateTime = true)
        {
            try
            {
                rwl.AcquireWriterLock(50);
                if (writeToConsole) { Console.WriteLine("[ERROR] " + customMessage + ": " + errorMessage); };
                File.AppendAllText(_consoleLog, writeDateTime ? DateTime.Now + "\t" + "[ERROR] \t" + customMessage + " [" + Thread.CurrentThread.ManagedThreadId + "]" + "\t" + errorMessage + Environment.NewLine :
                    "[" + Thread.CurrentThread.ManagedThreadId + "]" + "[ERROR] \t" + customMessage + "\t" + errorMessage + Environment.NewLine);
            }               
             finally
            {
                rwl.ReleaseWriterLock();
            }
            

        }

        public static void TraceLog(string traceMessage, bool isTaskFinished = false, bool writeDateTime = true)
        {
            try
            {
                rwl.AcquireWriterLock(100);
                if (isTaskFinished == false)
                {
                    Console.Write(traceMessage);
                    File.AppendAllText(_consoleLog, writeDateTime ? Environment.NewLine + DateTime.Now + " [" + Thread.CurrentThread.ManagedThreadId + "]" + "\t" + traceMessage : Environment.NewLine + "[" + Thread.CurrentThread.ManagedThreadId + "]" + traceMessage);
                }
                else if (isTaskFinished == true)
                {
                    Console.WriteLine(traceMessage);
                    File.AppendAllText(_consoleLog, traceMessage);
                }

            } finally
            {
                rwl.ReleaseWriterLock();
            }
        }

    }
}
