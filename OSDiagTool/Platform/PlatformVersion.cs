using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Platform {
    class PlatformUtils {

        public static string GetPlatformVersion(string osServerRegistry) {

            // Find Installation path and Platform Version 
            string osPlatformVersion = null;
            try {
                FileLogger.TraceLog("Verifying OutSystems Platform Version...");
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(osServerRegistry);

                //string osInstallationFolder = (string)OSPlatformInstaller.GetValue("");
                osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");
                FileLogger.TraceLog("Platform version: " + osPlatformVersion);

            } catch (Exception e) {
                FileLogger.LogError(" * Unable to find OutSystems Platform Server Installation... * ", e.Message + e.StackTrace);
                return null;
            }

            return osPlatformVersion;

        }

        public static string GetPlatformInstallationPath(string osServerRegistry) {

            try {
                FileLogger.TraceLog("Finding OutSystems Platform Installation Path...");

                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(osServerRegistry);
                string osInstallationFolder = (string)OSPlatformInstaller.GetValue("");

                return osInstallationFolder;

            } catch (Exception e) {
                FileLogger.LogError(" * Unable to find OutSystems Platform Version... * ", e.Message + e.StackTrace);
                return null;
            }

        }

    }
}
