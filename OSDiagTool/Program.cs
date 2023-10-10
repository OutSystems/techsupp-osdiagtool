using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.DatabaseExporter;
using OSDiagTool.OSDiagToolConf;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using System.Threading;

namespace OSDiagTool
{
    class Program
    {
        private static string _windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static string _tempFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "collect_data"); 
        private static string _targetZipFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "outsystems_data_" + DateTimeToTimestamp(DateTime.Now) + "_" + DateTime.Now.Second + DateTime.Now.Millisecond + ".zip");
        private static string _osInstallationFolder = @"C:\Program Files\OutSystems\Platform Server";
        private static string _iisApplicationHostPath = Path.Combine(_windir, @"system32\inetsrv\config\applicationHost.config");
        private static string _iisWebConfigPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), @"inetpub\wwwroot\web.config");
        private static string _machineConfigPath = Path.Combine(_windir, @"Microsoft.NET\Framework64\v4.0.30319\CONFIG\machine.config");
        private static string _evtVwrLogsDest = Path.Combine(_tempFolderPath, "EventViewerLogs");
        private static string _osPlatFilesDest = Path.Combine(_tempFolderPath, "Windows_And_OutSystems_Files");
        private static string _osMetamodelTablesDest = Path.Combine(_tempFolderPath, "PlatformMetamodelTables");
        private static string _windowsInfoDest = Path.Combine(_tempFolderPath, "WindowsInformation");
        private static string _errorDumpFile = Path.Combine(_tempFolderPath, "ConsoleLog.txt");
        private static string _osDatabaseTroubleshootDest = Path.Combine(_tempFolderPath, "DatabaseTroubleshoot");
        private static string _osPlatformLogs = Path.Combine(_tempFolderPath, "PlatformLogs");
        private static string _osPlatformDiagnostic = Path.Combine(_tempFolderPath, "PlatformDiagnostic");
        private static string _targetDiagnosticFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "diagnostic_" + DateTimeToTimestamp(DateTime.Now) + ".log");
        private static string _platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");
        private static string _appCmdPath = @"%windir%\system32\inetsrv\appcmd";

        // Registry paths
        private static string _netFrameworkRegistryPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP";
        private static string _outSystemsPlatformRegistryPath = @"SOFTWARE\OutSystems";
        private static string _osServerRegistry = @"SOFTWARE\OutSystems\Installer\Server";
        private static string _sslProtocolsRegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\Schannel\Protocols";
        private static string _iisRegistryPath = @"SOFTWARE\Microsoft\InetStp";
        private static string _rabbitMQRegistryPath = @"SOFTWARE\Ericsson\Erlang\ErlSrv\1.1\RabbitMQ";

        public static string privateKeyFilepath;
        public static string platformConfigurationFilepath;
        public static string osPlatformVersion;
        public static string dbEngine;
        public static string _endFeedback;
        public static bool separateLogCatalog;
        public static bool useMultiThread;

        static void Main(string[] args) {

            OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            var configurations = dgtConfReader.GetOsDiagToolConfigurations();
            // Override Multithread config from OSDiagToolConfReader
            if(WinPerfCounters.GetCPUUsage() > 2.0)
            {
                Program.useMultiThread = false;
            }

            try {
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_osServerRegistry);
                osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");
            } catch (Exception) {
                osPlatformVersion = null;
            }    

            if(osPlatformVersion == null) {
                Application.Run(new OSDiagToolForm.puf_popUpForm(OSDiagToolForm.puf_popUpForm._feedbackErrorType, "OutSystems Platform Server not found. "));
            }
            else {
                _osInstallationFolder = Platform.PlatformUtils.GetPlatformInstallationPath(_osServerRegistry);
                _platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");

                ConfigFileReader confFileParser = new ConfigFileReader(_platformConfigurationFilepath, osPlatformVersion);
                ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;
                dbEngine = platformDBInfo.DBMS.ToLower();

                var sqlConnString = new DBConnector.SQLConnStringModel();
                var orclConnString = new DBConnector.OracleConnStringModel();

                if (dbEngine.Equals("sqlserver"))
                {
                    sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                    sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;

                }
                else if (dbEngine.Equals("oracle"))
                {
                    orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                    orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                    orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                }

                // Checking if run is via CmdLine
                // args[0] RunCmdLine to run on CmdLine
                // args[1] saUser; args[2] sapwd;

                OSDiagToolInitialization();

                if (!args.Length.Equals(0))
                {
                    if (args[0].ToLower().Equals("runcmdline"))
                    {
                        if (args.Length.Equals(3))
                        { // One rune: runcmdline + saUser + saPwd

                            OSDGTCmdLine.CmdLineRun(configurations, platformDBInfo.DBMS, sqlConnString, orclConnString, args[1].ToString(), args[2].ToString());

                        }
                        else
                        {
                            OSDGTCmdLine.CmdLineRun(configurations, platformDBInfo.DBMS, sqlConnString, orclConnString);
                        }
                    }
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.Run(new OSDiagToolForm.OsDiagForm(configurations, platformDBInfo.DBMS, sqlConnString, orclConnString));
                }
            }
        }

        /* REFACTOR */
        public static void OSDiagToolInitialization() {

            // Reading serverhsconf and private key files
            // Creates a file to log traces during the execution
            // Delete temporary directory and all contents if it already exists (e.g.: error runs)
            if (Directory.Exists(_tempFolderPath)) {
                Directory.Delete(_tempFolderPath, true);
            }

            // Create temporary directory 
            Directory.CreateDirectory(_tempFolderPath);

            using (var errorTxtFile = File.Create(_errorDumpFile)) ;

            _osInstallationFolder = Platform.PlatformUtils.GetPlatformInstallationPath(_osServerRegistry);
            privateKeyFilepath = Path.Combine(_osInstallationFolder, "private.key");
            platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");
            osPlatformVersion = Platform.PlatformUtils.GetPlatformVersion(_osServerRegistry);
            separateLogCatalog = !osPlatformVersion.StartsWith("10.");
        }

        public static void GetPlatformAndServerFiles(CountdownEvent countdown = null /*used for multithread*/) {

            // Process Platform and Server Configuration files
            FileLogger.TraceLog("Copying Platform and Server configuration files... ");
            Directory.CreateDirectory(_osPlatFilesDest);
            Platform.PlatformFilesHelper.CopyPlatformAndServerConfFiles(_osInstallationFolder, _iisApplicationHostPath, _iisWebConfigPath, _machineConfigPath, _osPlatFilesDest);

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
                RegistryClass.RegistryCopy(_rabbitMQRegistryPath, Path.Combine(registryInformationPath, "RabbitMQ.txt"), true);

            } catch (Exception e) {
                FileLogger.LogError("Failed to export Registry:", e.Message + e.StackTrace);
            }
            finally
            {
                if (Program.useMultiThread) { countdown.Signal(); }
            }



        }

        public static void ExportEventViewerAndServerLogs(CountdownEvent countdown = null /*used for multithread*/)
        {

            WindowsEventLogHelper welHelper = new WindowsEventLogHelper();

            FileLogger.TraceLog("Exporting Event Viewer and Server logs... ");
            Directory.CreateDirectory(_evtVwrLogsDest);
            Directory.CreateDirectory(_windowsInfoDest);
            welHelper.GenerateLogFiles(Path.Combine(_tempFolderPath, _evtVwrLogsDest));
            ExecuteCommands();
            if (Program.useMultiThread) { countdown.Signal(); }
        }                       

        public static void CopyIISAccessLogs(int iisLogsNrDays, CountdownEvent countdown = null) {

            FileSystemHelper fsHelper = new FileSystemHelper();
            FileLogger.TraceLog(string.Format("Exporting IIS Access logs({0} days)...", iisLogsNrDays));
            IISHelper.GetIISAccessLogs(_iisApplicationHostPath, _tempFolderPath, fsHelper, iisLogsNrDays);
            if (Program.useMultiThread) { countdown.Signal(); }

        }

        public static void DatabaseTroubleshootProgram(OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel orclConnString = null, CountdownEvent countdown = null) {

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
            finally
            {
                if (Program.useMultiThread) { countdown.Signal(); }
            }
        }

        public static void ExportPlatformMetamodel(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations,
            DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel oracleConnString = null, CountdownEvent countdown = null) {

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

                ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
                string platformDBAdminUser = Platform.PlatformUtils.GetConfigurationValue("AdminUser", confFileParser.DBPlatformInfo); ;

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
            if (Program.useMultiThread) { countdown.Signal(); }
        }

        public static void ExportServiceCenterLogs(string dbEngine, OSDiagToolConf.ConfModel.strConfModel configurations, OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations,
            DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel oracleConnString = null, string adminSchema = null, CountdownEvent countdown = null) {

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
                Platform.LogExporter.PlatformLogExporter(dbEngine, platformLogs, FormConfigurations, _osPlatformLogs, configurations.queryTimeout, null, oracleConnString, adminSchema);
            }
            if (Program.useMultiThread) { countdown.Signal(); }
        }

        public static void CollectThreadDumpsProgram(bool getIisThreadDumps, bool getOsThreadDumps, CountdownEvent countdown = null) {

            FileLogger.TraceLog(string.Format("Initiating collection of thread dumps...(IIS thread dumps: {0} ; OutSystems Services thread dumps: {1})", getIisThreadDumps, getOsThreadDumps));
            CollectThreadDumps(getIisThreadDumps, getOsThreadDumps); // evaluate if this method is really necessary
            if (Program.useMultiThread) {
                countdown.Signal();
            }

        }

        public static void CollectMemoryDumpsProgram(bool getIisMemDumps, bool getOsMemDumps, CountdownEvent countdown = null) {

            FileLogger.TraceLog(string.Format("Initiating collection of thread dumps...(IIS memory dumps: {0} ; OutSystems Services memory dumps: {1})", getIisMemDumps, getOsMemDumps));
            CollectMemoryDumps(getIisMemDumps, getOsMemDumps);
            if (Program.useMultiThread) { countdown.Signal(); }

        }

        public static void GenerateZipFile() {

            FileLogger.TraceLog("Creating zip file... ");
            EventLog.WriteEntry("OSDiagTool", "Saving files to " + _targetZipFile);
            FileSystemHelper fsHelper = new FileSystemHelper();
            _targetZipFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "outsystems_data_" + DateTimeToTimestamp(DateTime.Now) + "_" + DateTime.Now.Second + DateTime.Now.Millisecond + ".zip"); // need to assign again in case the user runs the tool a second time
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

                }
            } catch (Exception e) {
                FileLogger.LogError("Failed to get thread dump: ", e.Message + e.StackTrace);
            }
        }

        /* 
         * Diagnose the OutSystems Platform
         */
        public static void PlatformDiagnosticProgram(OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel sqlConnString = null, 
            DBConnector.OracleConnStringModel oracleConnString = null, CountdownEvent countdown = null)
        {
            Directory.CreateDirectory(_osPlatformDiagnostic);
            try
            {
                FileLogger.TraceLog("Diagnosing the OutSystems Platform...");

                if (dbEngine.Equals("sqlserver"))
                {
                    Platform.PlatformDiagnostic.WriteLog(dbEngine, _osPlatformDiagnostic, configurations, sqlConnString, null);

                }
                else if (dbEngine.Equals("oracle"))
                {
                    Platform.PlatformDiagnostic.WriteLog(dbEngine, _osPlatformDiagnostic, configurations,null, oracleConnString);
                }
            }
            catch (Exception e)
            {
                FileLogger.LogError("Failed to diagnose the OutSystems Platform: ", e.Message + e.StackTrace);
            }
            finally
            {
                if (Program.useMultiThread) { countdown.Signal(); }
            }
        }

        public static void CollectMemoryDumps(bool iisMemDumps, bool osMemDumps)
        {
            List<string> processList = new List<string>();
            bool parentPrcNeedsResume = false;
            int parentProcessId = 0;

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

                        if(processName.Equals("w3wp.exe")) { // suspend parent svchost to prevent recycle

                            parentProcessId = WinPerfCounters.GetParentProcess(pid);
                            var svchostProcess = Process.GetProcessById(parentProcessId);
                            // TODO: implement suspend

                            Utils.WinUtils.SuspendProcess(parentProcessId);
                            parentPrcNeedsResume = true;

                        }

                        FileLogger.TraceLog(" - PID " + pid + " - ");
                        command = new CmdLineCommand("procdump64.exe -ma " + pid + " /accepteula " + "\"" + Path.Combine(memoryDumpsPath, filename) + "\"");
                        command.Execute();

                        if (parentPrcNeedsResume) Utils.WinUtils.ResumeProcess(parentProcessId);

                    }

                }
            } catch(Exception e) {
                FileLogger.LogError("Failed to get memory dump: ", e.Message + e.StackTrace);

            } finally {
                if (parentPrcNeedsResume) Utils.WinUtils.ResumeProcess(parentProcessId);
            }
            
        }

        private static string DateTimeToTimestamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMdd_HHmm");
        }
    }
}