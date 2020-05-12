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
using OSDiagTool.OSDiagToolForm;

namespace OSDiagTool
{
    class Program
    {
        private static string _windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static string _tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(),"collect_data"); 
        private static string _targetZipFile = Path.Combine(Directory.GetCurrentDirectory(), "outsystems_data_" + DateTimeToTimestamp(DateTime.Now) + "_" + DateTime.Now.Second + DateTime.Now.Millisecond + ".zip");
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
        private static string _osMetamodelTablesDest = Path.Combine(_tempFolderPath, "PlatformMetamodelTables");
        private static string _windowsInfoDest = Path.Combine(_tempFolderPath, "WindowsInformation");
        private static string _errorDumpFile = Path.Combine(_tempFolderPath, "ConsoleLog.txt");
        private static string _osDatabaseTroubleshootDest = Path.Combine(_tempFolderPath, "DatabaseTroubleshoot");
        private static string _osPlatformLogs = Path.Combine(_tempFolderPath, "PlatformLogs");
        private static string _platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");
        private static string _appCmdPath = @"%windir%\system32\inetsrv\appcmd";

        public static string privateKeyFilepath;
        public static string platformConfigurationFilepath;
        public static string osPlatformVersion;
        public static string dbEngine;
        public static string _endFeedback;
        public static bool separateLogCatalog;



        static void Main(string[] args) {

            OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            var configurations = dgtConfReader.GetOsDiagToolConfigurations();

            
            try {
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_osServerRegistry);
                osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");
            } catch (Exception e) {
                osPlatformVersion = null;
            }    

            if(osPlatformVersion == null) {

                Application.Run(new OSDiagToolForm.puf_popUpForm(OSDiagToolForm.puf_popUpForm._feedbackErrorType, "OutSystems Platform Server not found. "));

            }
            else {

                ConfigFileReader confFileParser = new ConfigFileReader(_platformConfigurationFilepath, osPlatformVersion);
                ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

                dbEngine = platformDBInfo.DBMS.ToLower();

                var sqlConnString = new DBConnector.SQLConnStringModel();
                var orclConnString = new DBConnector.OracleConnStringModel();

                if (dbEngine.Equals("sqlserver")) {

                    
                    sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                    sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;

                } else if (dbEngine.Equals("oracle")) {

                    orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                    orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                    orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                }

                Application.EnableVisualStyles();
                Application.Run(new OSDiagToolForm.OsDiagForm(configurations, platformDBInfo.DBMS, sqlConnString, orclConnString));

            }

        }

        /* REFACTOR */
        public static void OSDiagToolInitialization() {

            //Loading of configuration files should be returned by the initialization
            // Reading serverhsconf and private key files

            // Creates a file to log traces during the execution
            // Delete temporary directory and all contents if it already exists (e.g.: error runs)
            if (Directory.Exists(_tempFolderPath)) {
                Directory.Delete(_tempFolderPath, true);
            }

            // Create temporary directory 
            Directory.CreateDirectory(_tempFolderPath);

            using (var errorTxtFile = File.Create(_errorDumpFile)) ;

            privateKeyFilepath = Path.Combine(_osInstallationFolder, "private.key");
            platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");

            osPlatformVersion = Platform.PlatformUtils.GetPlatformVersion(_osServerRegistry);
            _osInstallationFolder = Platform.PlatformUtils.GetPlatformInstallationPath(_osServerRegistry);

            separateLogCatalog = !osPlatformVersion.StartsWith("10.");

            

        }

        public static void GetPlatformAndServerFiles() {

            // Process Platform and Server Configuration files
            FileLogger.TraceLog("Copying Platform and Server configuration files... ");
            Directory.CreateDirectory(_osPlatFilesDest);
            Platform.PlatformFilesHelper.CopyPlatformAndServerConfFiles(_osInstallationFolder, _iisApplicationHostPath, _machineConfigPath, _osPlatFilesDest);
            
        }

        public static void ExportEventViewerAndServerLogs() {

            WindowsEventLogHelper welHelper = new WindowsEventLogHelper();

            FileLogger.TraceLog("Exporting Event Viewer and Server logs... ");
            Directory.CreateDirectory(_evtVwrLogsDest);
            Directory.CreateDirectory(_windowsInfoDest);
            welHelper.GenerateLogFiles(Path.Combine(_tempFolderPath, _evtVwrLogsDest));
            ExecuteCommands();

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

            } catch (Exception e) {
                FileLogger.LogError("Failed to export Registry:", e.Message + e.StackTrace);
            }
            
        }                       

        public static void CopyIISAccessLogs(int iisLogsNrDays) {

            FileSystemHelper fsHelper = new FileSystemHelper();
            FileLogger.TraceLog(string.Format("Exporting IIS Access logs({0} days)...", iisLogsNrDays));
            IISHelper.GetIISAccessLogs(_iisApplicationHostPath, _tempFolderPath, fsHelper, iisLogsNrDays);
            
        }

        public static void DatabaseTroubleshootProgram(OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel orclConnString = null) {

            Directory.CreateDirectory(_osDatabaseTroubleshootDest);

            try {
                FileLogger.TraceLog(string.Format("Performing {0} Database Troubleshoot...", dbEngine.ToUpper()));

                if (dbEngine.Equals("sqlserver")) {
                    Database.DatabaseQueries.DatabaseTroubleshoot.DatabaseTroubleshooting(dbEngine, configurations, _osDatabaseTroubleshootDest, sqlConnString, null);

                }else if (dbEngine.Equals("oracle")) {
                    Database.DatabaseQueries.DatabaseTroubleshoot.DatabaseTroubleshooting(dbEngine, configurations, _osDatabaseTroubleshootDest, null, orclConnString);
                }

            } catch (Exception e) {
                FileLogger.LogError("Failed to perform Database Troubleshoot", e.Message + e.StackTrace);
            }
        }

        public static void ExportPlatformMetamodel(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations,
            DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel oracleConnString = null) {

            FileLogger.TraceLog("Exporting Platform Metamodel...");

            Directory.CreateDirectory(_osMetamodelTablesDest);

            if (dbEngine.Equals("sqlserver")) {

                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(sqlConnString);

                using (connection) {

                    bool isLifeTimeEnvironment = Platform.PlatformUtils.IsLifeTimeEnvironment(dbEngine, configurations.queryTimeout, connection);

                    FileLogger.TraceLog("Starting exporting tables: ");
                    foreach (string table in FormConfigurations.metamodelTables) {
                        if ((isLifeTimeEnvironment && table.ToLower().StartsWith("osltm") || table.ToLower().StartsWith("ossys"))) {
                            FileLogger.TraceLog(table + ", ", writeDateTime: false);
                            string selectAllQuery = "SELECT * FROM " + table;
                            CSVExporter.SQLToCSVExport(dbEngine, table, _osMetamodelTablesDest, configurations.queryTimeout, selectAllQuery, connection, null);
                        }
                    }

                }

            }
            else if (dbEngine.Equals("oracle")) {

                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(oracleConnString);

                string platformDBAdminUser = Platform.PlatformUtils.GetPlatformDBAdminUser();

                using (connection) {

                    bool isLifeTimeEnvironment = Platform.PlatformUtils.IsLifeTimeEnvironment(dbEngine, configurations.queryTimeout, null, connection, platformDBAdminUser);

                    FileLogger.TraceLog("Starting exporting tables: ");
                    foreach (string table in FormConfigurations.metamodelTables) {
                        if ((isLifeTimeEnvironment && table.ToLower().StartsWith("osltm") || table.ToLower().StartsWith("ossys"))) {
                            FileLogger.TraceLog(table + ", ", writeDateTime: false);
                            string selectAllQuery = "SELECT * FROM " + platformDBAdminUser + "." + table;
                            CSVExporter.ORCLToCsvExport(connection, table, _osMetamodelTablesDest, configurations.queryTimeout, platformDBAdminUser, selectAllQuery);
                        }
                    }
                }
            }
        }

        public static void ExportServiceCenterLogs(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations,
            DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel oracleConnString = null) {

            FileLogger.TraceLog(string.Format("Exporting Platform logs ({0} records)...", FormConfigurations.osLogTopRecords));

            Directory.CreateDirectory(_osPlatformLogs);

            List<string> platformLogs = new List<string>();
            foreach (string table in configurations.tableNames) { // add only oslog tables to list
                if (table.ToLower().StartsWith("oslog")) {
                    platformLogs.Add(table);
                }
            }

            if (dbEngine.Equals("sqlserver")) {
                Platform.LogExporter.PlatformLogExporter(dbEngine, platformLogs, FormConfigurations, _osPlatformLogs, configurations.queryTimeout, sqlConnString, null);
            } else if (dbEngine.Equals("oracle")) {
                Platform.LogExporter.PlatformLogExporter(dbEngine, platformLogs, FormConfigurations, _osPlatformLogs, configurations.queryTimeout, null, oracleConnString);
            }



        }

        public static void CollectThreadDumpsProgram(bool getIisThreadDumps, bool getOsThreadDumps) {

            FileLogger.TraceLog(string.Format("Initiating collection of thread dumps...(IIS thread dumps: {0} ; OutSystems Services thread dumps: {1})", getIisThreadDumps, getOsThreadDumps));
            CollectThreadDumps(getIisThreadDumps, getOsThreadDumps); // evaluate if this method is really necessary

        }

        public static void CollectMemoryDumpsProgram(bool getIisMemDumps, bool getOsMemDumps) {

            FileLogger.TraceLog(string.Format("Initiating collection of thread dumps...(IIS memory dumps: {0} ; OutSystems Services memory dumps: {1})", getIisMemDumps, getOsMemDumps));
            CollectMemoryDumps(getIisMemDumps, getOsMemDumps);

        }

        public static void GenerateZipFile() {

            FileLogger.TraceLog("Creating zip file... ");
            FileSystemHelper fsHelper = new FileSystemHelper();
            _targetZipFile = Path.Combine(Directory.GetCurrentDirectory(), "outsystems_data_" + DateTimeToTimestamp(DateTime.Now) + "_" + DateTime.Now.Second + DateTime.Now.Millisecond + ".zip"); // need to assign again in case the user runs the tool a second time
            fsHelper.CreateZipFromDirectory(_tempFolderPath, _targetZipFile, true);

            // Delete temp folder
            Directory.Delete(_tempFolderPath, true);

            _endFeedback = "File Location: " + _targetZipFile;
        }

        /* REFACTOR */

        private static void ExecuteCommands()
        {
            IDictionary<string, CmdLineCommand> commands = new Dictionary<string, CmdLineCommand>
            {
                { "appcmd " , new CmdLineCommand(string.Format("{0} list requests", _appCmdPath), Path.Combine(_tempFolderPath, "IISRequests.txt")) },
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
                FileLogger.LogError("Failed to get thread dump: ", e.Message + e.StackTrace);
            }


            }
            

        public static void CollectMemoryDumps(bool iisMemDumps, bool osMemDumps)
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
                        command = new CmdLineCommand("procdump64.exe -ma " + pid + " /accepteula " + "\"" + Path.Combine(memoryDumpsPath, filename) + "\"");
                        command.Execute();
                    }

                    FileLogger.TraceLog("DONE", true);
                }
            } catch(Exception e) {
                FileLogger.LogError("Failed to get memory dump: ", e.Message + e.StackTrace);
            }

            
        }

        private static string DateTimeToTimestamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMdd_HHmm");
        }
    }
}