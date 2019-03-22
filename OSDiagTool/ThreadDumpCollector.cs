using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Microsoft.Diagnostics.Runtime;


namespace OSDiagTool
{
    class ThreadDumpCollector
    {
        private AttachFlag _mode;
        private uint _attachTimeoutInMillis;


        public ThreadDumpCollector(uint attachTimeoutInMillis, AttachFlag mode)
        {
            AttachTimeoutInMillis = attachTimeoutInMillis;
            Mode = mode;
        }

        public ThreadDumpCollector(uint attachTimeoutInMillis) : this(attachTimeoutInMillis, AttachFlag.Passive) { }

       
        public string GetThreadDump(int pid)
        {
            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    using (var dataTarget = DataTarget.AttachToProcess(pid, _attachTimeoutInMillis, _mode))
                    {
                        writer.WriteLine(dataTarget.ClrVersions.First().Version);
                        var runtime = dataTarget.ClrVersions.First().CreateRuntime();

                        foreach (var domain in runtime.AppDomains)
                        {
                            writer.WriteLine("Domain " + domain.Name);
                        }
                        writer.WriteLine();
                        foreach (var t in runtime.Threads)
                        {
                            if (!t.IsAlive)
                                continue;

                            if (t.StackTrace.Count == 0)
                                continue;

                            writer.WriteLine("Thread " + t.ManagedThreadId + ": ");
                            int loop_count = 0;
                            foreach (var frame in t.EnumerateStackTrace())
                            {
                                writer.WriteLine("\t" + frame.StackPointer.ToString("x16") + " " + frame.ToString());
                                loop_count++;
                                if (loop_count > 200)
                                {
                                    writer.WriteLine("\t[CORRUPTED]");
                                    break;
                                }
                            }
                            writer.WriteLine();
                        }
                    }

                    return writer.ToString();
                }
                catch
                {
                    // This is mostly to catch the "invalid architecture" error.
                    // Any error that happens we want to ignore and return what we have.
                    return writer.ToString();
                }
            }
        }

        public List<Process> GetProcessesByName(string processName)
        {
            return new List<Process>(Process.GetProcessesByName(processName));
        }

        public List<int> GetProcessIdsByName(string processName)
        {
            List<int> processIds = new List<int>();

            foreach(Process p in Process.GetProcessesByName(processName))
            {
                processIds.Add(p.Id);
            }

            return processIds;
        }

        public List<int> GetProcessIdsByFilename(string processFilename)
        {
            List<int> processIds = new List<int>();

            foreach (Process p in Process.GetProcesses().Where(p => GetProcessFilename(p).EndsWith(processFilename)))
            {
                processIds.Add(p.Id);
            }

            return processIds;
        }

        public string GetProcessFilename(Process p)
        {
            try
            {
                return p.Modules[0].FileName;
            }
            catch
            {
                return "";
            }
        }

        public AttachFlag Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        public uint AttachTimeoutInMillis
        {
            get
            {
                return _attachTimeoutInMillis;
            }
            set
            {
                _attachTimeoutInMillis = value;
            }
        }

    }
}
