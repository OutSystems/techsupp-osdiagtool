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
        private static string _osDatabaseTablesDest = Path.Combine(_tempFolderPath, "DatabaseTables");
        private static string _windowsInfoDest = Path.Combine(_tempFolderPath, "WindowsInformation");
        private static string _errorDumpFile = Path.Combine(_tempFolderPath, "ConsoleLog.txt");
        private static string _osDatabaseTroubleshootDest = Path.Combine(_tempFolderPath, "DatabaseTroubleshoot");
        private static string _osPlatformLogs = Path.Combine(_tempFolderPath, "PlatformLogs");
        private static string _platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");
        private static string _appCmdPath = @"%windir%\system32\inetsrv\appcmd";
        public static string _endFeedback;


        static void Main(string[] args) {

            OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            var configurations = dgtConfReader.GetOsDiagToolConfigurations();

            string _osPlatformVersion;
            try {
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_osServerRegistry);
                _osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");
            } catch (Exception e) {
                _osPlatformVersion = null;
            }    

            if(_osPlatformVersion == null) {

                Application.Run(new OSDiagToolForm.puf_popUpForm(OSDiagToolForm.puf_popUpForm._feedbackErrorType, "OutSystems Platform Server not found. "));

            }
            else {

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

        }

        public static void RunOsDiagTool(OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations, OSDiagToolConf.ConfModel.strConfModel configurations) {


            int numberOfSteps = OSDiagToolHelper.CountSteps(FormConfigurations.cbConfs);

            puf_popUpForm popup = new puf_popUpForm(puf_popUpForm._feedbackWaitType, OsDiagForm._waitMessage, totalSteps: numberOfSteps + 1); // totalSteps + 1 for the zipping
            popup.Show();
            popup.Refresh();           

            // Initialize helper classes
            FileSystemHelper fsHelper = new FileSystemHelper();
            CmdHelper cmdHelper = new CmdHelper();
            WindowsEventLogHelper welHelper = new WindowsEventLogHelper();

            // Delete temporary directory and all contents if it already exists (e.g.: error runs)
            if (Directory.Exists(_tempFolderPath))
            {
                Directory.Delete(_tempFolderPath, true);
            }

            // Create temporary directory 
            Directory.CreateDirectory(_tempFolderPath);

            // Create error dump file to log all exceptions during script execution
            using (var errorTxtFile = File.Create(_errorDumpFile));

            string osPlatformVersion = Platform.PlatformVersion.GetPlatformVersion(_osServerRegistry);
 
            Object obj = RegistryClass.GetRegistryValue(_osServerRegistry, ""); // The "Defaut" values are empty strings.

            // Process Platform and Server Configuration files
            if (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._plPlatformAndServerFiles, out bool getPSandServerConfFiles) && getPSandServerConfFiles == true) {
                OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Copying Platform and Server configuration files...");
                FileLogger.TraceLog("Copying Platform and Server configuration files... ");
                Directory.CreateDirectory(_osPlatFilesDest);
                Platform.PlatformFilesHelper.CopyPlatformAndServerConfFiles(_osInstallationFolder, _iisApplicationHostPath, _machineConfigPath, _osPlatFilesDest);
                //CopyPlatformAndServerConfFiles();
            }

            // Export Event Viewer and Server Logs
            if (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._slEvt, out bool getEvt) && getEvt == true) {
                OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Exporting Event Viewer and Server logs...");
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

            // Reading serverhsconf and private key files
            string privateKeyFilepath = Path.Combine(_osInstallationFolder, "private.key");
            string platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");

            ConfigFileReader confFileParser = new ConfigFileReader(platformConfigurationFilepath, osPlatformVersion);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            ConfigFileDBInfo loggingDBInfo = null;
            bool separateLogCatalog = !osPlatformVersion.StartsWith("10.");
            if (separateLogCatalog) { // Log DB is on a separate DB Catalog starting Platform version 11
                loggingDBInfo = confFileParser.DBLoggingInfo;
            }

            // Retrieving IIS access logs
            if (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._slIisLogs, out bool getIisLogs) && getIisLogs == true) {
                OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, string.Format("Exporting IIS Access logs ({0} days)...", FormConfigurations.iisLogsNrDays));
                FileLogger.TraceLog(string.Format("Exporting IIS Access logs({0} days)...", FormConfigurations.iisLogsNrDays));

                IISHelper.GetIISAccessLogs(_iisApplicationHostPath, _tempFolderPath, fsHelper, FormConfigurations.iisLogsNrDays);
            }

            string dbEngine = platformDBInfo.DBMS;

            // Database Export
            if ((FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._diMetamodel, out bool _getPlatformMetamodel) && _getPlatformMetamodel == true) ||
                (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._diDbTroubleshoot, out bool _doDbTroubleshoot) && _doDbTroubleshoot == true) ||
                (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._plPlatformLogs, out bool _getPlatformLogs) && _getPlatformLogs == true)) {

                Directory.CreateDirectory(_osDatabaseTablesDest);
                Directory.CreateDirectory(_osDatabaseTroubleshootDest);
                Directory.CreateDirectory(_osPlatformLogs);

                try {

                    string platformDBRuntimeUser = platformDBInfo.GetProperty("RuntimeUser").Value;
                    string platformDBRuntimeUserPwd = platformDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));
                    string platformDBAdminUser = platformDBInfo.GetProperty("AdminUser").Value;

                    FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._diMetamodel, out bool getPlatformMetamodel);
                    FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._plPlatformLogs, out bool diGetPlatformLogs);
                    FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._diDbTroubleshoot, out bool doDbTroubleshoot);

                    if (dbEngine.ToLower().Equals("sqlserver")) {

                        var sqlConnString = new DBConnector.SQLConnStringModel();
                        string platformDBServer = sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                        string platformDBCatalog = sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;

                        // Database Troubleshoot: uses sa user
                        if (doDbTroubleshoot) {

                            OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Performing Database Troubleshoot...");
                            FileLogger.TraceLog("Performing SQL Server Database Troubleshoot...");

                            sqlConnString.userId = FormConfigurations.saUser;
                            sqlConnString.pwd = FormConfigurations.saPwd;

                            Database.DatabaseQueries.DatabaseTroubleshoot.DatabaseTroubleshooting(dbEngine, configurations, _osDatabaseTroubleshootDest, sqlConnString);
                        }


                        if (diGetPlatformLogs) {

                            OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, string.Format("Exporting Platform logs ({0} records)...", FormConfigurations.osLogTopRecords));
                            FileLogger.TraceLog(string.Format("Exporting Platform logs ({0} records)...", FormConfigurations.osLogTopRecords));

                            if (separateLogCatalog) {
                                sqlConnString.dataSource = loggingDBInfo.GetProperty("Server").Value;
                                sqlConnString.userId = loggingDBInfo.GetProperty("RuntimeUser").Value; // needs to use oslog configurations
                                sqlConnString.pwd = loggingDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));
                                sqlConnString.initialCatalog = loggingDBInfo.GetProperty("Catalog").Value;

                            }


                            List<string> platformLogs = new List<string>();
                            foreach (string table in configurations.tableNames) { // add only oslog tables to list
                                if (table.ToLower().StartsWith("oslog")) {
                                    platformLogs.Add(table);
                                }
                            }

                            Platform.LogExporter.PlatformLogExporter(dbEngine, platformLogs, FormConfigurations, _osPlatformLogs, configurations.queryTimeout, sqlConnString, null);

                        }

                        if (getPlatformMetamodel) {

                            OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Exporting Platform metamodel...");
                            FileLogger.TraceLog("Exporting Platform Metamodel...");

                            sqlConnString.dataSource = platformDBServer;
                            sqlConnString.userId = platformDBRuntimeUser; // needs to use oslog configurations
                            sqlConnString.pwd = platformDBRuntimeUserPwd;
                            sqlConnString.initialCatalog = platformDBCatalog;

                            var connector = new DBConnector.SLQDBConnector();
                            SqlConnection connection = connector.SQLOpenConnection(sqlConnString);

                            string _selectPlatSVCSObserver = "SELECT COUNT(ID) FROM OSSYS_PLATFORMSVCS_OBSERVER WHERE ISACTIVE = 1"; // check if it's registered on LifeTime

                            SqlCommand cmd = new SqlCommand(_selectPlatSVCSObserver, connection) {
                                CommandTimeout = configurations.queryTimeout
                            };

                            cmd.ExecuteNonQuery();
                            int count = Convert.ToInt32(cmd.ExecuteScalar());

                            using (connection) {
                                FileLogger.TraceLog("Starting exporting tables: ");
                                foreach (string table in FormConfigurations.metamodelTables) {
                                    if ((count.Equals(0) && table.ToLower().StartsWith("osltm") || table.ToLower().StartsWith("ossys"))) {
                                        FileLogger.TraceLog(table + ", ", writeDateTime: false);
                                        string selectAllQuery = "SELECT * FROM " + table;
                                        CSVExporter.SQLToCSVExport(dbEngine, table, _osDatabaseTablesDest, configurations.queryTimeout, selectAllQuery, connection, null);
                                    }
                                }
                            }
                        }

                    }   // Oracle Export -- RuntimeUser is used but the Admin schema is necessary to query OSSYS 
                    else if (dbEngine.ToLower().Equals("oracle")) {

                        

                        var orclConnString = new DBConnector.OracleConnStringModel();

                        string oraclePlatformDBHost = orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                        string oraclePlatformDBPort = orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                        string oraclePlatformDBServiceName = orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;

                        // Database Troubleshoot
                        if (doDbTroubleshoot) {

                            OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Performing Database Troubleshoot...");
                            FileLogger.TraceLog("Performing Oracle Database Troubleshoot...");

                            orclConnString.userId = FormConfigurations.saUser;
                            orclConnString.pwd = FormConfigurations.saPwd;

                            Database.DatabaseQueries.DatabaseTroubleshoot.DatabaseTroubleshooting(dbEngine, configurations, _osDatabaseTroubleshootDest, null, orclConnString);
                        }

                        if (diGetPlatformLogs) {

                            OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, string.Format("Exporting Platform logs ({0} records)...", FormConfigurations.osLogTopRecords));
                            FileLogger.TraceLog(string.Format("Exporting Platform logs ({0} records)...", FormConfigurations.osLogTopRecords));

                            if (separateLogCatalog) {

                                // needs to use oslog configurations
                                orclConnString.host = loggingDBInfo.GetProperty("Host").Value;
                                orclConnString.port = loggingDBInfo.GetProperty("Port").Value;
                                orclConnString.userId = loggingDBInfo.GetProperty("AdminUser").Value;
                                orclConnString.pwd = loggingDBInfo.GetProperty("AdminPassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));
                                orclConnString.serviceName = loggingDBInfo.GetProperty("ServiceName").Value;

                                List<string> platformLogs = new List<string>();
                                foreach (string table in configurations.tableNames) { // add only oslog tables to list
                                    if (table.ToLower().StartsWith("oslog")) {
                                        platformLogs.Add(table);
                                    }
                                }

                                Platform.LogExporter.PlatformLogExporter(dbEngine, platformLogs, FormConfigurations, _osPlatformLogs, configurations.queryTimeout, null, orclConnString);

                            }
                        }

                        if (getPlatformMetamodel) {

                            OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Exporting Platform metamodel...");
                            FileLogger.TraceLog("Exporting Platform Metamodel...");

                            orclConnString.userId = platformDBRuntimeUser;
                            orclConnString.pwd = platformDBRuntimeUserPwd;

                            var connector = new DBConnector.OracleDBConnector();
                            OracleConnection connection = connector.OracleOpenConnection(orclConnString);

                            string _selectPlatSVCSObserver = "SELECT COUNT(ID) FROM " + platformDBAdminUser + "." + "OSSYS_PLATFORMSVCS_OBSERVER WHERE ISACTIVE = 1";

                            OracleCommand cmd = new OracleCommand(_selectPlatSVCSObserver, connection) {
                                CommandTimeout = configurations.queryTimeout
                            };

                            cmd.ExecuteNonQuery();
                            int count = Convert.ToInt32(cmd.ExecuteScalar());

                            using (connection) {
                                FileLogger.TraceLog("Starting exporting tables: ");
                                foreach (string table in FormConfigurations.metamodelTables) {
                                    if ((count.Equals(0) && table.ToLower().StartsWith("osltm") || table.ToLower().StartsWith("ossys"))) {
                                        FileLogger.TraceLog(table + ", ", writeDateTime: false);
                                        string selectAllQuery = "SELECT * FROM " + platformDBAdminUser + "." + table;
                                        CSVExporter.ORCLToCsvExport(connection, table, _osDatabaseTablesDest, configurations.queryTimeout, platformDBAdminUser, selectAllQuery);
                                    }
                                }
                            }
                        }
                    }

                } catch (Exception e) {

                    FileLogger.LogError("Unable to export database tables", e.Message + e.StackTrace);

                }
                FileLogger.TraceLog("DONE", true);
            }


            if ((FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdIis, out bool getIisThreadDumps) && getIisThreadDumps == true) ||
                (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdOsServices, out bool _getOsThreadDumps) && _getOsThreadDumps == true)) {

                FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._tdOsServices, out bool getOsThreadDumps); // necessary because the first condition can be evaluated to true and second condition may never be checked

                OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Collecting thread dumps...");
                FileLogger.TraceLog(string.Format("Initiating collection of thread dumps...(IIS thread dumps: {0} ; OutSystems Services thread dumps: {1})", getIisThreadDumps, getOsThreadDumps));

                CollectThreadDumps(getIisThreadDumps, getOsThreadDumps);
            }

            if ((FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdIis, out bool getIisMemDumps) && getIisMemDumps == true) ||
                (FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdOsServices, out bool _getOsMemDumps) && _getOsMemDumps == true)) {

                FormConfigurations.cbConfs.TryGetValue(OSDiagToolForm.OsDiagForm._mdOsServices, out bool getOsMemDumps); // necessary because the first condition can be evaluated to true and second condition may never be checked

                OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Collecting memory dumps...");
                FileLogger.TraceLog(string.Format("Initiating collection of thread dumps...(IIS memory dumps: {0} ; OutSystems Services memory dumps: {1})", getIisMemDumps, getOsMemDumps));

                CollectMemoryDumps(getIisMemDumps, getOsMemDumps);
            }


            // Generate zip file
            OSDiagToolForm.puf_popUpForm.ChangeFeedbackLabelAndProgressBar(popup, "Zipping file...");
            FileLogger.TraceLog("Creating zip file... ");
            _targetZipFile = Path.Combine(Directory.GetCurrentDirectory(), "outsystems_data_" + DateTimeToTimestamp(DateTime.Now) + "_" + DateTime.Now.Second + DateTime.Now.Millisecond + ".zip"); // need to assign again in case the user runs the tool a second time
            fsHelper.CreateZipFromDirectory(_tempFolderPath, _targetZipFile, true);

            // Delete temp folder
            Directory.Delete(_tempFolderPath, true);

            popup.Dispose();
            popup.Close();

            _endFeedback = "File Location: " +_targetZipFile;


        }

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