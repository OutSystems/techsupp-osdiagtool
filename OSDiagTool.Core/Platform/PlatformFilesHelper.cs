using System;
using System.Collections.Generic;
using System.IO;

namespace OSDiagTool.Platform {
    class PlatformFilesHelper {

        private static string appPoolConfigPath = @"%SYSTEMDRIVE%\inetpub\temp\appPools";
        //  OS services config files location changed in the below versions from the Platform installation folder to Platform/<service>
        private static Version schedulerConfPathChangeVersion = new Version("11.11.1");
        private static Version deploymentConfPathChangeVersion = new Version("11.24.0");
        private static Version compilerConfPathChangeVersion = new Version("11.25.0");


        public static void CopyPlatformAndServerConfFiles(string _osInstallationFolder, string _iisApplicationHostPath, string _iisWebConfigPath, string _machineConfigPath, string _osPlatFilesDest, string osPlatformVersion) {

            string confFilePath = "";
            Version platformVersion = new Version(osPlatformVersion);

            // List of OS services and components
            IDictionary<string, string> osServiceNames = new Dictionary<string, string> {
                { "ConfigurationTool", "Configuration Tool" },
                { "LogServer", "Log Service" },
                { "CompilerService", "Deployment Controller Service" },
                { "DeployService", "Deployment Service" },
                { "Scheduler", "Scheduler Service" },
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

                switch (serviceFileEntry.Key)
                {
                    case @"Server.Identity\nlog": case @"Server.API\nlog":
                        confFilePath = Path.Combine(_osInstallationFolder, serviceFileEntry.Key + ".config");
                        break;

                    case "Scheduler":
                        if (platformVersion.CompareTo(schedulerConfPathChangeVersion) >= 0)
                        {
                            confFilePath = Path.Combine(_osInstallationFolder, serviceFileEntry.Key, serviceFileEntry.Key + ".exe.config");
                        } else { goto default; };
                        break;

                    case "CompilerService":
                        if (platformVersion.CompareTo(compilerConfPathChangeVersion) >= 0)
                        {
                            confFilePath = Path.Combine(_osInstallationFolder, serviceFileEntry.Key, serviceFileEntry.Key + ".exe.config");
                        }
                        else { goto default; };
                        break;

                    case "DeployService":
                        if (platformVersion.CompareTo(deploymentConfPathChangeVersion) >= 0)
                        {
                            confFilePath = Path.Combine(_osInstallationFolder, serviceFileEntry.Key, serviceFileEntry.Key + ".exe.config");
                        }
                        else { goto default; };
                        break;

                    default:
                        confFilePath = Path.Combine(_osInstallationFolder, serviceFileEntry.Key + ".exe.config");
                        break;
                }

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

            // Copy application pool configurations %SYSTEMDRIVE%\inetpub\temp\appPools
            FileSystemHelper fsHelper = new FileSystemHelper();
            appPoolConfigPath = fsHelper.GetPathWithWindowsRoot(appPoolConfigPath);
            fsHelper.DirectoryCopy(appPoolConfigPath, Path.Combine(_osPlatFilesDest, "AppPoolConfigurations"), true, false);

        }


    }
}
