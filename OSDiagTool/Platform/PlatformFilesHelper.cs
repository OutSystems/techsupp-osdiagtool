using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OSDiagTool.Platform {
    class PlatformFilesHelper {

        public static void CopyPlatformAndServerConfFiles(string _osInstallationFolder, string _iisApplicationHostPath, string _machineConfigPath, string _osPlatFilesDest) {

            string confFilePath;

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
            foreach (KeyValuePair<string, string> serviceFileEntry in osServiceNames) {

                confFilePath = Path.Combine(_osInstallationFolder, serviceFileEntry.Key + ".exe.config");

                // Get log file location from conf file
                OSServiceConfigFileParser confParser = new OSServiceConfigFileParser(serviceFileEntry.Value, confFilePath);
                string logPath = confParser.LogFilePath;

                // Add properties file
                files.Add(serviceFileEntry.Value + " config", confFilePath);
                
                // Add log file
                files.Add(serviceFileEntry.Value + " log", logPath);
            }

            // Copy all files to the temporary folder
            foreach (KeyValuePair<string, string> fileEntry in files) {
                String filepath = fileEntry.Value;
                String fileAlias = fileEntry.Key;

                FileLogger.TraceLog("Copying " + fileAlias + "... ");
                if (File.Exists(filepath)) {
                    String realFilename = Path.GetFileName(filepath);
                    File.Copy(filepath, Path.Combine(_osPlatFilesDest, realFilename));

                    FileLogger.TraceLog("DONE", true);
                } else {
                    FileLogger.TraceLog("(File does not exist)", true);
                }
            }
        }


    }
}
