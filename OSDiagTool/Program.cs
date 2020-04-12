using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using OSDiagTool.Utils;
using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.DatabaseExporter;
using OSDiagTool.OSDiagToolConf;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using System.Windows.Forms;

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
        private static string _osDatabaseTablesDest = Path.Combine(_tempFolderPath, "DatabaseTables");
        private static string _windowsInfoDest = Path.Combine(_tempFolderPath, "WindowsInformation");
        private static string _errorDumpFile = Path.Combine(_tempFolderPath, "ConsoleLog.txt");
        private static string _platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");


        static void Main(string[] args) {

            OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            var configurations = dgtConfReader.GetOsDiagToolConfigurations();

            RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_osServerRegistry);
            string _osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");

            ConfigFileReader confFileParser = new ConfigFileReader(_platformConfigurationFilepath, _osPlatformVersion);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            var sqlConnString = new DBConnector.SQLConnStringModel();
            var orclConnString = new DBConnector.OracleConnStringModel();

            if (platformDBInfo.DBMS.ToLower().Equals("sqlserver")) {

                sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;

            } else if (platformDBInfo.DBMS.ToLower().Equals("oracle")) {

                orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
            }

            Application.EnableVisualStyles();
            Application.Run(new OSDiagToolForm.OsDiagForm(configurations, platformDBInfo.DBMS, sqlConnString, orclConnString));

        }

        public static void RunOsDiagTool(OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations, OSDiagToolConf.ConfModel.strConfModel configurations) { // TODO: refactor this method for new input

            // Change console encoding to support all characters
            ////Console.OutputEncoding = Encoding.UTF8;

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
            Directory.CreateDirectory(_osDatabaseTablesDest);

            // Create error dump file to log all exceptions during script execution
            using (var errorTxtFile = File.Create(_errorDumpFile));


            Platform.PlatformVersion ps = new Platform.PlatformVersion();
            string osPlatformVersion = ps.GetPlatformVersion(_osServerRegistry);

            Object obj = RegistryClass.GetRegistryValue(_osServerRegistry, ""); // The "Defaut" values are empty strings.

            // Process copy files
            CopyAllFiles();

            // Generate text Event Viewer Logs
            if (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._slEvt, out bool getEvt) && getEvt == true) {
                FileLogger.TraceLog("Generating log files... ");
                welHelper.GenerateLogFiles(Path.Combine(_tempFolderPath, _evtVwrLogsDest));
                FileLogger.TraceLog("DONE", true);
                ExecuteCommands(); // FIX: Console popup

                // Export Registry information
                // Create directory for Registry information
                Directory.CreateDirectory(Path.Combine(_tempFolderPath, "RegistryInformation"));
                string registryInformationPath = Path.Combine(_tempFolderPath, "RegistryInformation");

                // Fetch Registry key values and subkeys values
                try {
                    FileLogger.TraceLog("Exporting Registry information...");

                    RegistryClass.RegistryCopy(_sslProtocolsRegistryPath, Path.Combine(registryInformationPath, "SSLProtocols.txt"), true);
                    RegistryClass.RegistryCopy(_netFrameworkRegistryPath, Path.Combine(registryInformationPath, "NetFramework.txt"), true);
                    RegistryClass.RegistryCopy(_iisRegistryPath, Path.Combine(registryInformationPath, "IIS.txt"), true);
                    RegistryClass.RegistryCopy(_outSystemsPlatformRegistryPath, Path.Combine(registryInformationPath, "OutSystemsPlatform.txt"), true);

                    FileLogger.TraceLog("DONE", true);
                } catch (Exception e) {
                    FileLogger.LogError("Failed to export Registry:", e.Message);
                }
            }
            
            // Reading serverhsconf and private key files
            string privateKeyFilepath = Path.Combine(_osInstallationFolder, "private.key");
            string platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");

            ConfigFileReader confFileParser = new ConfigFileReader(platformConfigurationFilepath, osPlatformVersion);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            ////OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            ////var configurations = dgtConfReader.GetOsDiagToolConfigurations();
            
            // Retrieving IIS access logs
            if (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._slIisLogs, out bool getIisLogs) && getIisLogs == true) {
                
                IISHelper.GetIISAccessLogs(_iisApplicationHostPath, _tempFolderPath, fsHelper, configurations.IISLogsNrDays);
            }


            // SQL Export
            if (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._diMetamodel, out bool getPlatformMetamodel) && getPlatformMetamodel == true) {
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

                        string _selectPlatSVCSObserver = "SELECT COUNT(ID) FROM OSSYS_PLATFORMSVCS_OBSERVER WHERE ISACTIVE = 1";

                        SqlCommand cmd = new SqlCommand(_selectPlatSVCSObserver, connection) {
                            CommandTimeout = configurations.queryTimeout
                        };
                        cmd.ExecuteNonQuery();
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        using (connection) {
                            FileLogger.TraceLog("Starting exporting tables: ");
                            foreach (string table in configurations.tableNames) {
                                if ((count.Equals(0) && table.ToLower().StartsWith("osltm") || table.ToLower().StartsWith("ossys"))) {
                                    FileLogger.TraceLog(table + ", ", writeDateTime: false);
                                    string selectAllQuery = "SELECT * FROM " + table;
                                    CSVExporter.SQLToCSVExport(connection, table, _osDatabaseTablesDest, configurations.queryTimeout, selectAllQuery);
                                }

                            }
                        }

                    }   // Oracle Export -- RuntimeUser is used but the Admin schema is necessary to query OSSYS 
                    else if (dbEngine.ToLower().Equals("oracle")) {
                        var orclConnString = new DBConnector.OracleConnStringModel();

                        orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                        orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                        orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                        orclConnString.userId = platformDBInfo.GetProperty("RuntimeUser").Value;
                        orclConnString.pwd = platformDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));
                        string osAdminSchema = platformDBInfo.GetProperty("AdminUser").Value;

                        var connector = new DBConnector.OracleDBConnector();
                        OracleConnection connection = connector.OracleOpenConnection(orclConnString);

                        string _selectPlatSVCSObserver = "SELECT COUNT(ID) FROM " + osAdminSchema + "." + "OSSYS_PLATFORMSVCS_OBSERVER WHERE ISACTIVE = 1";

                        OracleCommand cmd = new OracleCommand(_selectPlatSVCSObserver, connection) {
                            CommandTimeout = configurations.queryTimeout
                        };
                        cmd.ExecuteNonQuery();
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        using (connection) {
                            FileLogger.TraceLog("Starting exporting tables: ");
                            foreach (string table in configurations.tableNames) {
                                if ((count.Equals(0) && table.ToLower().StartsWith("osltm") || table.ToLower().StartsWith("ossys"))) {
                                    FileLogger.TraceLog(table + ", ", writeDateTime: false);
                                    string selectAllQuery = "SELECT * FROM " + osAdminSchema + "." + table;
                                    CSVExporter.ORCLToCsvExport(connection, table, _osDatabaseTablesDest, configurations.queryTimeout, osAdminSchema, selectAllQuery);
                                }
                            }
                        }

                    }

                } catch (Exception e) {

                    FileLogger.LogError("Unable to export database tables", e.Message);

                }
                FileLogger.TraceLog("DONE", true);
            }

            
            if ((FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdIis, out bool getIisThreadDumps) && getIisThreadDumps == true) || 
                (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdOsServices, out bool _getOsThreadDumps) &&_getOsThreadDumps == true)) {

                FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdOsServices, out bool getOsThreadDumps); // necessary because the first condition can be evaluated to true and second condition may never be checked
                CollectThreadDumps(getIisThreadDumps, getOsThreadDumps);
            }


            ////Console.Write("Do you want to collect memory dumps? (y/N) ");
            if ((FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdIis, out bool getIisMemDumps) && getIisMemDumps == true) ||
                (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdOsServices, out bool _getOsMemDumps) && _getOsMemDumps == true)) {

                FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdOsServices, out bool getOsMemDumps); // necessary because the first condition can be evaluated to true and second condition may never be checked

                FileLogger.TraceLog("Initiating collection of memory dumps..." + Environment.NewLine);
                CollectMemoryDumps(getIisMemDumps, getOsMemDumps);
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
                { "app_evtx", new CmdLineCommand("WEVTUtil epl Application " + "\"" + Path.Combine(_tempFolderPath, _evtVwrLogsDest + @"\Application.evtx") + "\"") },
                { "sys_evtx", new CmdLineCommand("WEVTUtil epl System " + "\"" + Path.Combine(_tempFolderPath, _evtVwrLogsDest + @"\System.evtx") + "\"") },
                { "sec_evtx", new CmdLineCommand("WEVTUtil epl Security " + "\"" + Path.Combine(_tempFolderPath, _evtVwrLogsDest + @"\Security.evtx") + "\"") }
            };

            foreach (KeyValuePair<string, CmdLineCommand> commandEntry in commands)
            {
                FileLogger.TraceLog("Getting " + commandEntry.Key + "...");
                commandEntry.Value.Execute();
                FileLogger.TraceLog("DONE" + Environment.NewLine);
            }
        }

        private static void CollectThreadDumps(bool iisThreads, bool osThreads)
        {
            List<string> processList = new List<string>();

            if (iisThreads && osThreads) {
                processList.AddRange(new List<string>() {"w3wp", "deployment_controller", "deployment_service", "scheduler", "log_service" });
            } else if (!iisThreads && osThreads) {
                processList.AddRange(new List<string>() {"deployment_controller", "deployment_service", "scheduler", "log_service" });
            } else if (iisThreads && !osThreads) {
                processList.AddRange(new List<string>() { "w3wp" });
            } else {
                return;
            }

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

            try {
                foreach (string processTag in processList) {
                    FileLogger.TraceLog("Collecting " + processTag + " thread dumps... ");

                    string processName = processDict[processTag];
                    List<int> pids = dc.GetProcessIdsByName(processName);

                    foreach (int pid in dc.GetProcessIdsByFilename(processName)) {
                        string pidSuf = pids.Count > 1 ? "_" + pid : "";
                        string filename = "threads_" + processTag + pidSuf + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
                        using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(threadDumpsPath, filename)))) {
                            writer.WriteLine(DateTime.Now.ToString());
                            writer.WriteLine(dc.GetThreadDump(pid));
                        }
                    }

                    FileLogger.TraceLog("DONE", true);
                }
            } catch (Exception e) {
                FileLogger.LogError("Failed to get thread dump: ", e.Message);
            }


            }
            

        private static void CollectMemoryDumps(bool iisMemDumps, bool osMemDumps)
        {

            List<string> processList = new List<string>();

            if (iisMemDumps && osMemDumps) {
                processList.AddRange(new List<string>() { "w3wp", "deployment_controller", "deployment_service", "scheduler", "log_service" });
            } else if (!iisMemDumps && osMemDumps) {
                processList.AddRange(new List<string>() { "deployment_controller", "deployment_service", "scheduler", "log_service" });
            } else if (iisMemDumps && !osMemDumps) {
                processList.AddRange(new List<string>() { "w3wp" });
            } else {
                return;
            }

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

            try {

                foreach (string processTag in processList) {
                    FileLogger.TraceLog("Collecting " + processTag + " memory dumps... ");

                    string processName = processDict[processTag];
                    List<int> pids = dc.GetProcessIdsByName(processName);

                    foreach (int pid in dc.GetProcessIdsByFilename(processName)) {
                        string pidSuf = pids.Count > 1 ? "_" + pid : "";
                        string filename = "memdump_" + processTag + pidSuf + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dmp";

                        FileLogger.TraceLog(" - PID " + pid + " - ");
                        command = new CmdLineCommand("procdump64.exe -ma " + pid + " /accepteula " + Path.Combine(memoryDumpsPath, filename));
                        command.Execute();
                    }

                    FileLogger.TraceLog("DONE", true);
                }
            } catch(Exception e) {
                FileLogger.LogError("Failed to get memory dump: ", e.Message);
            }

            
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