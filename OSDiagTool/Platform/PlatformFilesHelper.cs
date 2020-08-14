using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OSDiagTool.Platform {
    class PlatformFilesHelper {

        public static void CopyPlatformAndServerConfFiles(string _osInstallationFolder, string _iisApplicationHostPath, string _iisWebConfigPath, string _machineConfigPath, string _osPlatFilesDest) {

            string confFilePath = "";

            // List of OS services and components
            IDictionary<string, string> osServiceNames = new Dictionary<string, string> {
                { "ConfigurationTool", "Configuration Tool" },
                { "LogServer", "Log Service" },
                { "CompilerService", "Deployment Controller Service" },
                { "DeployService", "Deployment Service" },
                { "Scheduler", "Scheduler Service" },
                { "SMSConnector", "SMS Service" }
            };

            if (!Program.osPlatformVersion.StartsWith("10.")) {
                osServiceNames.Add(@"Server.API\nlog", "Server API");
                osServiceNames.Add(@"Server.Identity\nlog", "Server Identity");
            }

            // Initialize dictionary with all the files that we need to get and can be accessed directly
            IDictionary<string, string> files = new Dictionary<string, string> {
                { "ServerHSConf", Path.Combine(_osInstallationFolder, "server.hsconf") },
                { "OSVersion", Path.Combine(_osInstallationFolder, "version.txt") },
                { "applicationHost.config", _iisApplicationHostPath },
                { "machine.config", _machineConfigPath },
                { "web.config", _iisWebConfigPath }
            };

            // Add OS log and configuration files
            foreach (KeyValuePair<string, string> serviceFileEntry in osServiceNames) {

                bool isNlogConfFile = serviceFileEntry.Key.ToLower().Contains(@"\nlog");

                confFilePath = isNlogConfFile ? Path.Combine(_osInstallationFolder, serviceFileEntry.Key + ".config") : Path.Combine(_osInstallationFolder, serviceFileEntry.Key + ".exe.config"); // Check if it's server API or identity

                // Get log file location from conf file
                OSServiceConfigFileParser confParser = new OSServiceConfigFileParser(serviceFileEntry.Value, confFilePath);

                string logPath = isNlogConfFile ? confParser.LogServerAPIAndIdentityFilePath : confParser.LogFilePath;

                // Add properties file
                files.Add(serviceFileEntry.Value + " config", confFilePath);

                // Add log file
                files.Add(serviceFileEntry.Value + " log", logPath);

            }

            // Copy all files to the temporary folder
            foreach (KeyValuePair<string, string> fileEntry in files) {
                String filepath = fileEntry.Value;
                String fileAlias = fileEntry.Key;
                bool isNlogConfFile = false;

                if (filepath != null) {
                    isNlogConfFile = filepath.ToLower().Contains("nlog.config");
                }

                FileLogger.TraceLog("Copying " + fileAlias + "... ");
                if (File.Exists(filepath)) {
                    String realFilename = isNlogConfFile ? Path.GetFileName(filepath) + "_" + fileAlias : Path.GetFileName(filepath);
                    File.Copy(filepath, Path.Combine(_osPlatFilesDest, realFilename));

                } else {
                    FileLogger.TraceLog("(File does not exist)", true);
                }
            }

            // Export Environment variables
            foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables()) {
                string name = (string)env.Key;
                string value = (string)env.Value;

                Utils.WinUtils.WriteToFile(Path.Combine(_osPlatFilesDest, "EnvironmentVariables.txt"), name + ": " + value);
                
            }
        }


    }
}
