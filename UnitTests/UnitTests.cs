using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using OSDiagTool.Core.DBConnector;
using OSDiagTool.Core.OSDiagToolConf;
using OSDiagTool.Core;
using OSDiagTool.Core.Database;
using OSDiagTool.Core.Platform;
using OSDiagTool.Core.Platform.ConfigFiles;
using OSDiagTool.Core.Utils;

/* TEST INSTRUCTIONS
 * Run Visual Studio in Administrator mode
 * Configure Processor Architecture to x64: Test > Processor Architecture for AnyCPU Projects > x64
*/

namespace lUnitTests
{
    [TestFixture]
    public class ProgramUnitTest
    {

        private static SQLConnStringModel sqlConnString = new SQLConnStringModel();
        private static OracleConnStringModel orclConnString = new OracleConnStringModel();
        private static OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
        private static ConfModel.strConfModel configurations = new ConfModel.strConfModel();
        private static string oracleAdminSchema;

        [Test]
        public void Test_GetPlatformAndServerFiles()
        {
            Program.GetPlatformAndServerFiles();
            string registryInformationPath = Path.Combine(Program._tempFolderPath, "RegistryInformation");

            List<string> directoryPaths = new List<string> { Program._tempFolderPath, Program._osPlatFilesDest , registryInformationPath };
            List<string> filePaths = new List<string> { "SSLProtocols.txt", "NetFramework.txt", "IIS.txt", "OutSystemsPlatform.txt", "RabbitMQ.txt" };

            foreach (string path in directoryPaths)
            {
                Assert.That(Directory.Exists(path));
            }

            foreach (string path in filePaths)
            {
                Assert.That(File.Exists(Path.Combine(registryInformationPath, path)));
            }
        }

        [Test]
        public void Test_ExportEventViewerlogs()
        {
            Program.ExportEventViewerAndServerLogs();

            List<string> directoryPaths = new List<string> { Program._evtVwrLogsDest, Program._windowsInfoDest };
            List<string> eventLogsFilePaths = new List<string> { "Application.evtx", "System.evtx", "Security.evtx", "EventViewerLog_Application.log", "EventViewerLog_System.log" };
            List<string> windowsFilePaths = new List<string> { "cpu_info", "dir_outsystems", "env_info", "mem_cache", "memory_info", "net_protocol", "os_info", "pagefile", "partition", "startup", "tasklist" };

            foreach (string path in directoryPaths)
            {
                Assert.That(Directory.Exists(path));
            }

            foreach (string path in eventLogsFilePaths)
            {
                Assert.That(File.Exists(Path.Combine(Program._evtVwrLogsDest, path)));
            }
            
            foreach (string path in windowsFilePaths)
            {
                Assert.That(File.Exists(Path.Combine(Program._windowsInfoDest, path)));
            }
        }

        [Test]
        public void Test_CopyIISAccessLogs()
        {
            // Test > Processor Architecture for AnyCPU Projects > x64 to be successful
            Program.CopyIISAccessLogs(60);

            string iisLogsPath = Path.Combine(Program._tempFolderPath, "IISLogs");

            Assert.That(Directory.Exists(iisLogsPath));
            Assert.That(SearchFiles(iisLogsPath, ".log")); // Checks if just one .log file exists in sub-directories
        }

        [Test]
        public void Test_DatabaseTroubleshoot()
        {
            if (Program.dbEngine.Equals(DatabaseType.SqlServer))
            {
                Program.DatabaseTroubleshootProgram(configurations, sqlConnString);
            }
            else if (Program.dbEngine.Equals(DatabaseType.Oracle))
            {
                Program.DatabaseTroubleshootProgram(configurations, null, orclConnString);
            }

            Assert.That(SearchFiles(Program._osDatabaseTroubleshootDest, ".csv")); // Checks if just one .csv file exists in sub-directories
        }

        [Test]
        public void Test_CollectThreadDumpsProgram()
        {
            Program.CollectThreadDumpsProgram(true, true);

            Assert.That(Directory.Exists(Program.threadDumpsPath));
            Assert.That(SearchFiles(Program.threadDumpsPath, ".log")); // Checks if just one.log file exists in sub - directories

        }

        [Test]
        public void Test_CollectMemoryDumpsProgram()
        {
            Program.CollectMemoryDumpsProgram(true, true);

            Assert.That(Directory.Exists(Program.memoryDumpsPath));
            Assert.That(SearchFiles(Program.memoryDumpsPath, ".dmp")); // check if one .dmp file exists in sub-directories

        }

        [Test]
        public void Test_PlatformIntegrityCheckProgram()
        {
            Program.PlatformIntegritycheck(configurations, sqlConnString, orclConnString, oracleAdminSchema);

            Assert.That(Directory.Exists(Program.platDBIntCheckPath));

            foreach (string key in PlatformDBIntegrity.integrityModel.CheckDetails.Keys)
            {
                Assert.That(PlatformDBIntegrity.integrityModel.CheckDetails[key].checkOk is null); // dictionary value is a nullable bool. If the check ran OK, it was set to true or false
            }

            foreach (string key in PlatformDBIntegrity.ServerChecks.Keys)
            {
                Assert.That(PlatformDBIntegrity.ServerChecks[key] is null);
            }
        }

        static bool SearchFiles(string directory, string searchString)
        {
            bool foundFile = false;
            try
            {
                // Search for files with the specified string in their names in the current directory.
                string[] files = Directory.GetFiles(directory, $"*{searchString}*");

                foreach (string file in files)
                {
                    if (file.Contains(searchString)){ foundFile = true; break; };
                }

                // Recursively search in subdirectories.
                string[] subdirectories = Directory.GetDirectories(directory);
                foreach (string subdirectory in subdirectories)
                {
                    if (foundFile) { break; };
                    if(SearchFiles(subdirectory, searchString).Equals(true))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
            }
            if (foundFile)
            {
                return true;
            }
            return false;
            
        }

        [SetUp]
        public void OSDGToolInitialization()
        {
            //OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
            configurations = dgtConfReader.GetOsDiagToolConfigurations();

            Program.useMultiThread = false; // override to not use multithread - prevents null pointers in the tests
            string privateKeyFilepath = Path.Combine(Program._osInstallationFolder, "private.key");

            try
            {
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(Program._osServerRegistry);
                Program.osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");
            }
            catch (Exception)
            {
                Program.osPlatformVersion = null;
            }

            Program._osInstallationFolder = PlatformUtils.GetPlatformInstallationPath(Program._osServerRegistry);
            Program._platformConfigurationFilepath = Path.Combine(Program._osInstallationFolder, "server.hsconf");

            ConfigFileReader confFileParser = new ConfigFileReader(Program._platformConfigurationFilepath, Program.osPlatformVersion);
            ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;
            string dbEngineString = platformDBInfo.DBMS.ToLower();

            if (dbEngineString.Equals("sqlserver")) {
                Program.dbEngine = DatabaseType.SqlServer;
            } else if (dbEngineString.Equals("oracle"))
            {
                Program.dbEngine = DatabaseType.Oracle;
            }

            if (Program.dbEngine.Equals(DatabaseType.SqlServer))
            {
                sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;
                sqlConnString.userId = platformDBInfo.GetProperty("AdminUser").Value;
                sqlConnString.pwd = platformDBInfo.GetProperty("AdminPassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));

            }
            else if (Program.dbEngine.Equals(DatabaseType.Oracle))
            {
                orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                orclConnString.userId = platformDBInfo.GetProperty("AdminUser").Value;
                orclConnString.pwd = platformDBInfo.GetProperty("AdminPassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));

                PlatformConnectionStringDefiner ConnectionStringDefiner = new PlatformConnectionStringDefiner();
                PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, false, false, ConnectionStringDefiner);
                string oracleAdminSchema = ConnStringHelper.AdminSchema;
            }
            
            Program.OSDiagToolInitialization();

        }

    }

}

