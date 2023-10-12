using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OSDiagTool;
using System.IO;
using System.Collections.Generic;

namespace OSDiagToolUnitTests
{
    [TestClass]
    public class ProgramUnitTest
    {
        [TestMethod]
        public void Test_GetPlatformAndServerFiles()
        {
            //Program instance = new OSDiagTool.Program();

            Program.OSDiagToolInitialization(); //OSDiagToolInitialization must be called in to initialize the program and create output folders
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

            Program.OSDiagToolInitialization(); //OSDiagToolInitialization must be called in to initialize the program and create output folders
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
            // REVIEW: test fails due to unable to find applicationHost.config
            Program.OSDiagToolInitialization(); //OSDiagToolInitialization must be called in to initialize the program and create output folders
            Program.CopyIISAccessLogs(60);

            string iisLogsPath = Path.Combine(OSDiagTool.Program._tempFolderPath, "IISLogs");

           Assert.IsTrue(Directory.Exists(iisLogsPath));
           Assert.IsNotNull(Directory.GetFiles(Path.Combine(iisLogsPath, "*log")));

        }
    }
}
