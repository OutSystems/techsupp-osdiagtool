using Microsoft.Win32;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.Platform.ConfigFiles;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public static bool IsLifeTimeEnvironment(string dbEngine, int queryTimeout, SqlConnection SqlConnection = null, OracleConnection orclConnection = null, string platformDBAdminUser = null) {

            if (dbEngine.Equals("sqlserver")) {

                string _selectPlatSVCSObserver = "SELECT COUNT(ID) FROM OSSYS_PLATFORMSVCS_OBSERVER WHERE ISACTIVE = 1"; // check if it's registered on LifeTime. If it isn't, assume it's LifeTime

                SqlCommand cmd = new SqlCommand(_selectPlatSVCSObserver, SqlConnection) {
                    CommandTimeout = queryTimeout
                };

                cmd.ExecuteNonQuery();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count.Equals(0)) {
                    return true;
                }

            }
            else if (dbEngine.Equals("oracle")) {

                string _selectPlatSVCSObserver = "SELECT COUNT(ID) FROM " + platformDBAdminUser + "." + "OSSYS_PLATFORMSVCS_OBSERVER WHERE ISACTIVE = 1";

                OracleCommand cmd = new OracleCommand(_selectPlatSVCSObserver, orclConnection) {
                    CommandTimeout = queryTimeout
                };

                cmd.ExecuteNonQuery();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count.Equals(0)) {
                    return true;
                }
            }

            return false;

        }

        public static string GetPlatformDBAdminUser() {

            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            return platformDBInfo.GetProperty("AdminUser").Value;

        }
    }
}
