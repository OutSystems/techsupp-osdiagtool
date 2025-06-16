using Microsoft.Win32;
using System.Reflection;
using OSDiagTool.Core.Database;
using OSDiagTool.Core.OSDiagToolConf;
using OSDiagTool.Core.Platform;
using OSDiagTool.Core.Platform.ConfigFiles;
using OSDiagTool.Core.DBConnector;

namespace OSDiagToolUI
{
    public class Program
    {
        private static string _windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        public static string _tempFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "collect_data"); 
        private static string _targetZipFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "outsystems_data_" + DateTimeToTimestamp(DateTime.Now) + "_" + DateTime.Now.Second + DateTime.Now.Millisecond + ".zip");
        public static string _osInstallationFolder = @"C:\Program Files\OutSystems\Platform Server";
        public static string _iisApplicationHostPath = Path.Combine(_windir, @"system32\inetsrv\config\applicationHost.config");
        private static string _iisWebConfigPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), @"inetpub\wwwroot\web.config");
        private static string _machineConfigPath = Path.Combine(_windir, @"Microsoft.NET\Framework64\v4.0.30319\CONFIG\machine.config");
        public static string _evtVwrLogsDest = Path.Combine(_tempFolderPath, "EventViewerLogs");
        public static string _osPlatFilesDest = Path.Combine(_tempFolderPath, "Windows_And_OutSystems_Files");
        private static string _osMetamodelTablesDest = Path.Combine(_tempFolderPath, "PlatformMetamodelTables");
        public static string _windowsInfoDest = Path.Combine(_tempFolderPath, "WindowsInformation");
        private static string _errorDumpFile = Path.Combine(_tempFolderPath, "ConsoleLog.txt");
        public static string _osDatabaseTroubleshootDest = Path.Combine(_tempFolderPath, "DatabaseTroubleshoot");
        private static string _osPlatformLogs = Path.Combine(_tempFolderPath, "PlatformLogs");
        private static string _osPlatformDiagnostic = Path.Combine(_tempFolderPath, "NetworkDiagnostic");
        private static string _targetDiagnosticFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "diagnostic_" + DateTimeToTimestamp(DateTime.Now) + ".log");
        public static string _platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");
        private static string _appCmdPath = @"%windir%\system32\inetsrv\appcmd";
        public static int serverProcessorCount = Environment.ProcessorCount;
        public static string threadDumpsPath = Path.Combine(_tempFolderPath, "ThreadDumps");
        public static string memoryDumpsPath = Path.Combine(_tempFolderPath, "MemoryDumps");
        public static string platDBIntCheckPath = Path.Combine(_tempFolderPath, "PlatformIntegrity");

        // Registry paths
        private static string _netFrameworkRegistryPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP";
        private static string _outSystemsPlatformRegistryPath = @"SOFTWARE\OutSystems";
        public static string _osServerRegistry = @"SOFTWARE\OutSystems\Installer\Server";
        private static string _sslProtocolsRegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\Schannel\Protocols";
        private static string _iisRegistryPath = @"SOFTWARE\Microsoft\InetStp";
        private static string _rabbitMQRegistryPath = @"SOFTWARE\Ericsson\Erlang\ErlSrv\1.1\RabbitMQ";

        public static string privateKeyFilepath;
        public static string platformConfigurationFilepath;
        public static string osPlatformVersion;
        public static DatabaseType dbEngine;
        public static string _endFeedback;
        public static bool separateLogCatalog;
        public static bool useMultiThread;

        static void Main(string[] args) {

            OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            var configurations = dgtConfReader.GetOsDiagToolConfigurations();

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
                _osInstallationFolder = PlatformUtils.GetPlatformInstallationPath(_osServerRegistry);
                _platformConfigurationFilepath = Path.Combine(_osInstallationFolder, "server.hsconf");

                ConfigFileReader confFileParser = new ConfigFileReader(_platformConfigurationFilepath, osPlatformVersion);
                ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;
                string dbEngineString = platformDBInfo.DBMS.ToLower();

                var sqlConnString = new SQLConnStringModel();
                var orclConnString = new OracleConnStringModel();

                if (dbEngineString.Equals("sqlserver"))
                {
                    dbEngine = DatabaseType.SqlServer;
                    sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                    sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;

                }
                else if (dbEngineString.Equals("oracle"))
                {
                    dbEngine = DatabaseType.Oracle;
                    orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                    orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                    orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                }

                // Checking if run is via CmdLine
                // args[0] RunCmdLine to run on CmdLine
                // args[1] saUser; args[2] sapwd;

                OSDiagTool.Core.Program.OSDiagToolInitialization();

                Application.EnableVisualStyles();
                Application.Run(new OSDiagToolForm.OsDiagForm(configurations, dbEngine, sqlConnString, orclConnString));
                
            }
        }                

        private static string DateTimeToTimestamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMdd_HHmm");
        }
    }
}