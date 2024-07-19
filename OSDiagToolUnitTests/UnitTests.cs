using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OSDiagTool;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using OSDiagTool.OSDiagToolConf;
using OSDiagTool.Platform.ConfigFiles;

/* TEST INSTRUCTIONS
 * Run Visual Studio in Administrator mode
 * Configure Processor Architecture to x64: Test > Processor Architecture for AnyCPU Projects > x64 to be successful
 * The program expected user input with Database credentials. Change the credentials in [TestInitialize] (<user id> and <pwd>) to connect to the database. Sandboxes and local databases are not public but avoid commits with these
*/

namespace OSDiagToolUnitTests
{
    [TestClass]
    public class ProgramUnitTest
    {

        private static OSDiagTool.DBConnector.SQLConnStringModel sqlConnString = new OSDiagTool.DBConnector.SQLConnStringModel();
        private static OSDiagTool.DBConnector.OracleConnStringModel orclConnString = new OSDiagTool.DBConnector.OracleConnStringModel();
        private static OSDiagToolConfReader dgtConfReader = new OSDiagToolConfReader();
        private static OSDiagTool.OSDiagToolConf.ConfModel.strConfModel configurations = new OSDiagTool.OSDiagToolConf.ConfModel.strConfModel();
        private static string oracleAdminSchema;

        [TestMethod]
        public void Test_GetPlatformAndServerFiles()
        {
            Program.GetPlatformAndServerFiles();
            string registryInformationPath = Path.Combine(OSDiagTool.Program._tempFolderPath, "RegistryInformation");

            List<string> directoryPaths = new List<string> { OSDiagTool.Program._tempFolderPath, OSDiagTool.Program._osPlatFilesDest , registryInformationPath };
            List<string> filePaths = new List<string> { "SSLProtocols.txt", "NetFramework.txt", "IIS.txt", "OutSystemsPlatform.txt", "RabbitMQ.txt" };

            foreach (string path in directoryPaths)
            {
                Assert.IsTrue(Directory.Exists(path));
            }

            foreach (string path in filePaths)
            {
                Assert.IsTrue(File.Exists(Path.Combine(registryInformationPath, path)));
            }
        }

        [TestMethod]
        public void Test_ExportEventViewerlogs()
        {
            Program.ExportEventViewerAndServerLogs();

            List<string> directoryPaths = new List<string> { OSDiagTool.Program._evtVwrLogsDest, OSDiagTool.Program._windowsInfoDest };
            List<string> eventLogsFilePaths = new List<string> { "Application.evtx", "System.evtx", "Security.evtx", "EventViewerLog_Application.log", "EventViewerLog_System.log" };
            List<string> windowsFilePaths = new List<string> { "cpu_info", "dir_outsystems", "env_info", "mem_cache", "memory_info", "net_protocol", "os_info", "pagefile", "partition", "startup", "tasklist" };

            foreach (string path in directoryPaths)
            {
                Assert.IsTrue(Directory.Exists(path));
            }

            foreach (string path in eventLogsFilePaths)
            {
                Assert.IsTrue(File.Exists(Path.Combine(OSDiagTool.Program._evtVwrLogsDest, path)));
            }
            
            foreach (string path in windowsFilePaths)
            {
                Assert.IsTrue(File.Exists(Path.Combine(OSDiagTool.Program._windowsInfoDest, path)));
            }
        }

        [TestMethod]
        public void Test_CopyIISAccessLogs()
        {
            // Test > Processor Architecture for AnyCPU Projects > x64 to be successful
            Program.CopyIISAccessLogs(60);

            string iisLogsPath = Path.Combine(OSDiagTool.Program._tempFolderPath, "IISLogs");

            Assert.IsTrue(Directory.Exists(iisLogsPath));
            Assert.IsTrue(SearchFiles(iisLogsPath, ".log")); // Checks if just one .log file exists in sub-directories
        }

        [TestMethod]
        public void Test_DatabaseTroubleshoot()
        {
            if (Program.dbEngine.Equals(OSDiagTool.Database.DatabaseType.SqlServer))
            {
                Program.DatabaseTroubleshootProgram(configurations, sqlConnString);
            }
            else if (Program.dbEngine.Equals(OSDiagTool.Database.DatabaseType.Oracle))
            {
                Program.DatabaseTroubleshootProgram(configurations, null, orclConnString);
            }

            Assert.IsTrue(SearchFiles(OSDiagTool.Program._osDatabaseTroubleshootDest, ".csv")); // Checks if just one .csv file exists in sub-directories
        }

        [TestMethod]
        public void Test_CollectThreadDumpsProgram()
        {
            Program.CollectThreadDumpsProgram(true, true);

            Assert.IsTrue(Directory.Exists(Program.threadDumpsPath));
            Assert.IsTrue(SearchFiles(Program.threadDumpsPath, ".log")); // Checks if just one.log file exists in sub - directories

        }

        [TestMethod]
        public void Test_CollectMemoryDumpsProgram()
        {
            Program.CollectMemoryDumpsProgram(true, true);

            Assert.IsTrue(Directory.Exists(Program.memoryDumpsPath));
            Assert.IsTrue(SearchFiles(Program.memoryDumpsPath, ".dmp")); // check if one .dmp file exists in sub-directories

        }

        [TestMethod]
        public void Test_PlatformIntegrityCheckProgram()
        {
            Program.PlatformIntegritycheck(configurations, sqlConnString, orclConnString, oracleAdminSchema);

            Assert.IsTrue(Directory.Exists(Program.platDBIntCheckPath));

            foreach (string key in OSDiagTool.Platform.PlatformDBIntegrity.integrityModel.CheckDetails.Keys)
            {
                Assert.IsFalse(OSDiagTool.Platform.PlatformDBIntegrity.integrityModel.CheckDetails[key].checkOk is null); // dictionary value is a nullable bool. If the check ran OK, it was set to true or false
            }

            foreach (string key in OSDiagTool.Platform.PlatformDBIntegrity.ServerChecks.Keys)
            {
                Assert.IsFalse(OSDiagTool.Platform.PlatformDBIntegrity.ServerChecks[key] is null);
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

        [TestInitialize]
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

            Program._osInstallationFolder = OSDiagTool.Platform.PlatformUtils.GetPlatformInstallationPath(Program._osServerRegistry);
            Program._platformConfigurationFilepath = Path.Combine(Program._osInstallationFolder, "server.hsconf");

            ConfigFileReader confFileParser = new ConfigFileReader(Program._platformConfigurationFilepath, Program.osPlatformVersion);
            ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;
            string dbEngineString = platformDBInfo.DBMS.ToLower();

            if (dbEngineString.Equals("sqlserver")) {
                Program.dbEngine = OSDiagTool.Database.DatabaseType.SqlServer;
            } else if (dbEngineString.Equals("oracle"))
            {
                Program.dbEngine = OSDiagTool.Database.DatabaseType.Oracle;
            }

            if (Program.dbEngine.Equals(OSDiagTool.Database.DatabaseType.SqlServer))
            {
                sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;
                sqlConnString.userId = platformDBInfo.GetProperty("AdminUser").Value;
                sqlConnString.pwd = platformDBInfo.GetProperty("AdminPassword").GetDecryptedValue(OSDiagTool.Utils.CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));

            }
            else if (Program.dbEngine.Equals(OSDiagTool.Database.DatabaseType.Oracle))
            {
                orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                orclConnString.userId = platformDBInfo.GetProperty("AdminUser").Value;
                orclConnString.pwd = platformDBInfo.GetProperty("AdminPassword").GetDecryptedValue(OSDiagTool.Utils.CryptoUtils.GetPrivateKeyFromFile(privateKeyFilepath));

                OSDiagTool.Platform.PlatformConnectionStringDefiner ConnectionStringDefiner = new OSDiagTool.Platform.PlatformConnectionStringDefiner();
                OSDiagTool.Platform.PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, false, false, ConnectionStringDefiner);
                string oracleAdminSchema = ConnStringHelper.AdminSchema;
            }
            
            Program.OSDiagToolInitialization();

        }

    }

}

