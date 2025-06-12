using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace OSDiagTool
{
    public class WindowsEventLogHelper
    {
        protected string _pathToEventLogs = string.Format(@"{0}\winevt\logs", Environment.SystemDirectory);
        private IDictionary<string, EventLog> eventLogs;
        private List<string> logNames;

        public WindowsEventLogHelper(bool loadEventsOnInit = true)
        {
            // can't read Security log :(
            //logNames = new List<string> { "System", "Application", "Security" };
            logNames = new List<string> { "System", "Application"};
            eventLogs = new Dictionary<string, EventLog>();

            if (loadEventsOnInit)
                LoadEvents();
        }

        protected void LoadEvents()
        {
            EventLog[] logs = EventLog.GetEventLogs();

            foreach(EventLog log in logs)
            {
                if (logNames.Contains(log.Log))
                {
                    eventLogs.Add(log.Log, log);
                }
            }
        }

        public bool EventLogExists(string logName)
        {
            return EventLog.Exists(logName);
        }

        public string GetEventLogFilepath(string logName)
        {
            return Path.Combine(_pathToEventLogs, logName + ".evtx");
        }

        public void GenerateLogFile(string logName, string targetLogFile, EventLogEntryType logLevel = EventLogEntryType.Information)
        {
            if (eventLogs.ContainsKey(logName))
            {
                using (FileStream fs = File.Create(targetLogFile))
                using (StreamWriter wfs = new StreamWriter(fs))
                {
                    foreach (EventLogEntry logEntry in eventLogs[logName].Entries)
                    {
                        if (logEntry.EntryType <= logLevel)
                        {
                            wfs.WriteLine(string.Format("{0} [{1}] {2}", logEntry.TimeGenerated.ToString(), logEntry.EntryType.ToString(), logEntry.Message));
                        }
                    }
                }
            }
        }


        public void GenerateLogFiles(string targetFolder)
        {
            foreach(string logName in logNames)
            {
                string targetFile = Path.Combine(targetFolder, "EventViewerLog_" + logName + ".log");
                GenerateLogFile(logName, targetFile);
            }
        }
    }
}
