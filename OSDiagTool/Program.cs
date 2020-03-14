using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Linq;
using OSDiagTool.Utils;
using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.DatabaseExporter;
using OSDiagTool.OSDiagToolConf;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

namespace OSDiagTool
{
    class Program
    {
        private static string _windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static string _tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(),"collect_data"); 
        private static string _targetZipFile = Path.Combine(Directory.GetCurrentDirectory(), "outsystems_data_" + DateTimeToTimestamp(DateTime.Now) + ".zip");
        private static string _osInstallationFolder = @"C:\Program Files\OutSystems\Platform Server";
        private static string _osServerRegistry = @"SOFTWARE\OutSystems\Installer\Server";
        private static string _sslProtocolsRegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\Schannel\Protocols";
        private static string _iisRegistryPath = @"SOFTWARE\Microsoft\InetStp";
        private static string _netFrameworkRegistryPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP";
        private static string _outSystemsPlatformRegistryPath = @"SOFTWARE\OutSystems";
        private static string _iisApplicationHostPath = Path.Combine(_windir, @"system32\inetsrv\config\applicationHost.config");
        private static string _machineConfigPath = Path.Combine(_windir, @"Microsoft.NET\Framework64\v4.0.30319\CONFIG\machine.config");
        private static string _evtVwrLogsDest = Path.Combine(_tempFolderPath, "EventViewerLogs");
        private static string _osPlatFilesDest = Path.Combine(_tempFolderPath, "OSPlatformFiles");
        private static string _windowsInfoDest = Path.Combine(_tempFolderPath, "WindowsInformation");
        private static string _errorDumpFile = Path.Combine(_tempFolderPath, "ConsoleLog.txt");

        static void Main(string[] args)
        {
            // Change console encoding to support all characters
            Console.OutputEncoding = Encoding.UTF8;

            // Initialize helper classes
            FileSystemHelper fsHelper = new FileSystemHelper();
            CmdHelper cmdHelper = new CmdHelper();
            WindowsEventLogHelper welHelper = new WindowsEventLogHelper();

            // Delete temporary directory and all contents if it already exists (e.g.: error runs)
            if (Directory.Exists(_tempFolderPath))
            {
                Directory.Delete(_tempFolderPath, true);
            }

            // Create temporary directory and respective subdirectories
            Directory.CreateDirectory(_tempFolderPath);
            Directory.CreateDirectory(_evtVwrLogsDest);
            Directory.CreateDirectory(_osPlatFilesDest);
            Directory.CreateDirectory(_windowsInfoDest);

            // Create error dump file to log all exceptions during script execution
            using (var errorTxtFile = File.Create(_errorDumpFile));

            // Finding Installation folder 
                try
            {
                FileLogger.TraceLog("Finding OutSystems Platform Installation Path...");
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_osServerRegistry);
                
                _osInstallationFolder = (string) OSPlatformInstaller.GetValue("");
                FileLogger.TraceLog("Found it on: " + _osInstallationFolder, true);
            }
            catch (Exception e)
            {
                FileLogger.LogError(" * Unable to find OutSystems Platform Server Installation... * ", e.Message);
                WriteExitLines();
                return;
            } 


            Object obj = RegistryClass.GetRegistryValue(_osServerRegistry, ""); // The "Defaut" values are empty strings.

            // Process copy files
            CopyAllFiles();

            // Generate Event Viewer Logs
            FileLogger.TraceLog("Generating log files... ");
            welHelper.GenerateLogFiles(Path.Combine(_tempFolderPath, _evtVwrLogsDest));
            FileLogger.TraceLog("DONE", true);

            ExecuteCommands();



            // Reading OSDGTool config file
            string privateKeyFilepath = Path.Combine(_osInstallationFolder, "private.key");
            string platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");

            ConfigFileReader confFileParser = new ConfigFileReader(platformConfigurationFilepath);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            var configurations = dgtConfReader.GetOsDiagToolConfigurations();

            //Retrieving IIS access logs
            IISHelper.GetIISAccessLogs(_iisApplicationHostPath, _tempFolderPath, fsHelper, configurations.IISLogsNrDays);

            // Export Registry information
            // Create directory for Registry information
            Directory.CreateDirectory(Path.Combine(_tempFolderPath, "RegistryInformation"));
            string registryInformationPath = Path.Combine(_tempFolderPath, "RegistryInformation");

            // Fetch Registry key values and subkeys values
            try
            {
                FileLogger.TraceLog("Exporting Registry information...");

                RegistryClass.RegistryCopy(_sslProtocolsRegistryPath, Path.Combine(registryInformationPath, "SSLProtocols.txt"), true);
                RegistryClass.RegistryCopy(_netFrameworkRegistryPath, Path.Combine(registryInformationPath, "NetFramework.txt"), true);
                RegistryClass.RegistryCopy(_iisRegistryPath, Path.Combine(registryInformationPath, "IIS.txt"), true);
                RegistryClass.RegistryCopy(_outSystemsPlatformRegistryPath, Path.Combine(registryInformationPath, "OutSystemsPlatform.txt"), true);

                FileLogger.TraceLog("DONE", true);
            }
            
            catch (Exception e)
            {
                FileLogger.LogError("Failed to export Registry:", e.Message);
            }

            // SQL Export -- fix There is already an open DataReader associated with this Command which must be closed first.
            try {

                
                string dbEngine = platformDBInfo.DBMS;
                if (dbEngine.ToLower().Equals("sqlserver")) {

                    var sqlConnString = new DBConnector.SQLConnStringModel();
                    sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                    sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;
                    sqlConnString.userId = platformDBInfo.GetProperty("RuntimeUser").Value;
                    sqlConnString.pwd = platformDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));

                    var connector = new DBConnector.SLQDBConnector();
                    SqlConnection connection = connector.SQLOpenConnection(sqlConnString);

                    using (connection) {
                        FileLogger.TraceLog("Starting exporting tables: ");
                        foreach (string table in configurations.tableNames) {
                            FileLogger.TraceLog(table + ", ", writeDateTime: false);
                            CSVExporter.SQLToCSVExport(connection, table, _tempFolderPath, configurations.queryTimeout);
                        }
                    }

                }   // Oracle Export -- AdminUser schema is necessary to query OSSYS
                else if (dbEngine.ToLower().Equals("oracle")) {
                    var orclConnString = new DBConnector.OracleConnStringModel();

                    orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                    orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                    orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                    orclConnString.userId = platformDBInfo.GetProperty("AdminUser").Value;
                    orclConnString.pwd = platformDBInfo.GetProperty("AdminPassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));

                    var connector = new DBConnector.OracleDBConnector();
                    OracleConnection connection = connector.OracleOpenConnection(orclConnString);

                    using (connection) {
                        FileLogger.TraceLog("Starting exporting tables: ");
                        foreach (string table in configurations.tableNames) {
                            FileLogger.TraceLog(table + ", ", writeDateTime: false);
                            CSVExporter.ORCLToCsvExport(connection, table, _tempFolderPath, configurations.queryTimeout);
                        }
                    }
                    
                }
                
            } catch (Exception e) {

                FileLogger.LogError("Unable to export database tables", e.Message);

            }
            FileLogger.TraceLog("DONE", true);


            // Collect thread dumps - TODO ask y/n
            CollectThreadDumps();

            Console.Write("Do you want to collect memory dumps? (y/N) ");
            string mem_dump_input = Console.ReadLine().ToLower();

            if (string.Equals(mem_dump_input, "y"))
            {
                FileLogger.TraceLog("Initiating collection of memory dumps..." + Environment.NewLine);
                CollectMemoryDumps();
            }

            // Generate zip file
            Console.WriteLine();
            FileLogger.TraceLog("Creating zip file... ");
            fsHelper.CreateZipFromDirectory(_tempFolderPath, _targetZipFile, true);
            Console.WriteLine("DONE");

            // Delete temp folder
            Directory.Delete(_tempFolderPath, true);

            // Print process end
            PrintEnd();
            WriteExitLines();
        }

        // write a generic exit line and wait for user input
        private static void WriteExitLines()
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void CopyAllFiles()
        {
            // List of OS services and components
            IDictionary<string, string> osServiceNames = new Dictionary<string, string> {
                { "ConfigurationTool", "Configuration Tool" },
                { "LogServer", "Log Service" },
                { "CompilerService", "Deployment Controller Service" },
                { "DeployService", "Deployment Service" },
                { "Scheduler", "Scheduler Service" },
                { "SMSConnector", "SMS Service" }
            };

            // Initialize dictionary with all the files that we need to get and can be accessed directly
            IDictionary<string, string> files = new Dictionary<string, string> {
                { "ServerHSConf", Path.Combine(_osInstallationFolder, "server.hsconf") },
                { "OSVersion", Path.Combine(_osInstallationFolder, "version.txt") },
                { "applicationHost.config", _iisApplicationHostPath },
                { "machine.config", _machineConfigPath }
            };

            // Add OS log and configuration files
            foreach (KeyValuePair<string, string> serviceFileEntry in osServiceNames)
            {
                string confFilePath = Path.Combine(_osInstallationFolder, serviceFileEntry.Key + ".exe.config");

                // Get log file location from conf file
                OSServiceConfigFileParser confParser = new OSServiceConfigFileParser(serviceFileEntry.Value, confFilePath);
                string logPath = confParser.LogFilePath;

                // Add properties file
                files.Add(serviceFileEntry.Value + " config", confFilePath);

                // Add log file
                files.Add(serviceFileEntry.Value + " log", logPath);
            }

            // Copy all files to the temporary folder
            foreach (KeyValuePair<string, string> fileEntry in files)
            {
                String filepath = fileEntry.Value;
                String fileAlias = fileEntry.Key;

                FileLogger.TraceLog("Copying " + fileAlias + "... ");
                if (File.Exists(filepath))
                {
                    String realFilename = Path.GetFileName(filepath);
                    File.Copy(filepath, Path.Combine(_osPlatFilesDest, realFilename));

                    FileLogger.TraceLog("DONE", true);
                }
                else
                {
                    FileLogger.TraceLog("(File does not exist)", true);
                }
            }
        }

        private static void ExecuteCommands()
        {
            IDictionary<string, CmdLineCommand> commands = new Dictionary<string, CmdLineCommand>
            {
                { "dir_outsystems", new CmdLineCommand(string.Format("dir /s /a \"{0}\"", _osInstallationFolder), Path.Combine(_windowsInfoDest, "dir_outsystems")) },
                { "tasklist", new CmdLineCommand("tasklist /v", Path.Combine(_windowsInfoDest, "tasklist")) },
                { "cpu_info", new CmdLineCommand("wmic cpu", Path.Combine(_windowsInfoDest, "cpu_info")) },
                { "memory_info", new CmdLineCommand("wmic memphysical", Path.Combine(_windowsInfoDest, "memory_info")) },
                { "mem_cache", new CmdLineCommand("wmic memcache", Path.Combine(_windowsInfoDest, "mem_cache")) },
                { "net_protocol", new CmdLineCommand("wmic netprotocol", Path.Combine(_windowsInfoDest, "net_protocol")) },
                { "env_info", new CmdLineCommand("wmic environment", Path.Combine(_windowsInfoDest, "env_info")) },
                { "os_info", new CmdLineCommand("wmic os", Path.Combine(_windowsInfoDest, "os_info")) },
                { "pagefile", new CmdLineCommand("wmic pagefile", Path.Combine(_windowsInfoDest, "pagefile")) },
                { "partition", new CmdLineCommand("wmic partition", Path.Combine(_windowsInfoDest, "partition")) },
                { "startup", new CmdLineCommand("wmic startup", Path.Combine(_windowsInfoDest, "startup")) },
                { "app_evtx", new CmdLineCommand("WEVTUtil epl Application " + Path.Combine(_tempFolderPath, _evtVwrLogsDest + @"\Application.evtx")) },
                { "sys_evtx", new CmdLineCommand("WEVTUtil epl System " + Path.Combine(_tempFolderPath, _evtVwrLogsDest + @"\System.evtx")) },
                { "sec_evtx", new CmdLineCommand("WEVTUtil epl Security " + Path.Combine(_tempFolderPath, _evtVwrLogsDest + @"\Security.evtx")) }
            };

            foreach (KeyValuePair<string, CmdLineCommand> commandEntry in commands)
            {
                FileLogger.TraceLog("Getting " + commandEntry.Key + "...");
                commandEntry.Value.Execute();
                FileLogger.TraceLog("DONE" + Environment.NewLine);
            }
        }

        private static void CollectThreadDumps()
        {
            string threadDumpsPath = Path.Combine(_tempFolderPath, "ThreadDumps");
            Directory.CreateDirectory(threadDumpsPath);

            ThreadDumpCollector dc = new ThreadDumpCollector(5000);
            Dictionary<string, string> processDict = new Dictionary<string, string>{
                { "log_service", "LogServer.exe" },
                { "deployment_service", "DeployService.exe" },
                { "deployment_controller", "CompilerService.exe" },
                { "scheduler", "Scheduler.exe" },
                { "w3wp", "w3wp.exe" }
            };

            List<string> processList = new List<string> { "w3wp", "deployment_controller", "deployment_service", "scheduler", "log_service" };

            foreach (string processTag in processList)
            {
                FileLogger.TraceLog("Collecting " + processTag + " thread dumps... ");

                string processName = processDict[processTag];
                List<int> pids = dc.GetProcessIdsByName(processName);

                foreach (int pid in dc.GetProcessIdsByFilename(processName))
                {
                    string pidSuf = pids.Count > 1 ? "_" + pid : "";
                    string filename = "threads_" + processTag + pidSuf + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
                    using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(threadDumpsPath, filename))))
                    {
                        writer.WriteLine(DateTime.Now.ToString());
                        writer.WriteLine(dc.GetThreadDump(pid));
                    }
                }

                FileLogger.TraceLog("DONE", true);
            }
        }

        private static void CollectMemoryDumps()
        {
            string memoryDumpsPath = Path.Combine(_tempFolderPath, "MemoryDumps");
            Directory.CreateDirectory(memoryDumpsPath);

            CmdLineCommand command;

            ThreadDumpCollector dc = new ThreadDumpCollector(5000);
            Dictionary<string, string> processDict = new Dictionary<string, string>{
                { "log_service", "LogServer.exe" },
                { "deployment_service", "DeployService.exe" },
                { "deployment_controller", "CompilerService.exe" },
                { "scheduler", "Scheduler.exe" },
                { "w3wp", "w3wp.exe" }
            };

            List<string> processList = new List<string> { "w3wp", "deployment_controller", "deployment_service", "scheduler", "log_service" };

            foreach (string processTag in processList)
            {
                FileLogger.TraceLog("Collecting " + processTag + " memory dumps... ");

                string processName = processDict[processTag];
                List<int> pids = dc.GetProcessIdsByName(processName);

                foreach (int pid in dc.GetProcessIdsByFilename(processName))
                {
                    string pidSuf = pids.Count > 1 ? "_" + pid : "";
                    string filename = "memdump_" + processTag + pidSuf + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dmp";

                    FileLogger.TraceLog(" - PID " + pid + " - " ); 
                    command = new CmdLineCommand("procdump64.exe -ma " + pid + " /accepteula " + Path.Combine(memoryDumpsPath, filename));
                    command.Execute();
                }

                FileLogger.TraceLog("DONE", true);
            }
        }

        private static void GetServiceCenterLogs(string platformInstallationFolder)
        {
            string privateKeyFilepath = Path.Combine(platformInstallationFolder, "private.key");
            string platformConfigurationFilepath = Path.Combine(platformInstallationFolder, "server.hsconf");

            ConfigFileReader confFileParser = new ConfigFileReader(platformConfigurationFilepath);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            Console.Write("Getting Service Center logs: TODO!!!...");

            
            // Oracle
            /*Console.WriteLine(platformDBInfo.DBMS);
            Console.WriteLine(platformDBInfo.GetProperty("Host").Value);
            Console.WriteLine(platformDBInfo.GetProperty("Port").Value);
            Console.WriteLine(platformDBInfo.GetProperty("ServiceName").Value);
            Console.WriteLine(platformDBInfo.GetProperty("AdminUser").Value);
            Console.WriteLine(platformDBInfo.GetProperty("AdminPassword").Value);
            Console.WriteLine(platformDBInfo.GetProperty("AdminPassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath)));
            Console.ReadKey();*/

            // SQL Server
            Console.WriteLine(platformDBInfo.DBMS);
            Console.WriteLine(platformDBInfo.GetProperty("Server").Value);
            Console.WriteLine(platformDBInfo.GetProperty("Catalog").Value);
            Console.WriteLine(platformDBInfo.GetProperty("AdminUser").Value);
            Console.WriteLine(platformDBInfo.GetProperty("AdminPassword").Value);
            Console.WriteLine(platformDBInfo.GetProperty("AdminPassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath)));
            Console.ReadKey();
            bool test = platformDBInfo.GetProperty("bla").IsEncrypted;
            

            Console.WriteLine("DONE");

            return;
        }

        private static void PrintEnd()
        {
            Console.WriteLine();
            Console.WriteLine("OSDiagTool data collection has finished. Resulting zip file path:");
            Console.WriteLine(_targetZipFile);
            Console.WriteLine();
        }

        private static string DateTimeToTimestamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMdd_HHmm");
        }
    }
}