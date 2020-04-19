using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Platform {
    class PlatformVersion {

        public static string GetPlatformVersion(string osServerRegistry) {

            // Find Installation path and Platform Version 
            string osPlatformVersion = null;
            try {
                FileLogger.TraceLog("Finding OutSystems Platform Installation Path...");
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(osServerRegistry);

                string osInstallationFolder = (string)OSPlatformInstaller.GetValue("");
                osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");
                FileLogger.TraceLog("Found it on: " + osInstallationFolder + "; Version: " + osPlatformVersion, true);

            } catch (Exception e) {
                FileLogger.LogError(" * Unable to find OutSystems Platform Server Installation... * ", e.Message + e.StackTrace);
                WriteExitLines();
                return null;
            }

            return osPlatformVersion;

        }

        private static void WriteExitLines() {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
