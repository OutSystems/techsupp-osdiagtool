using Microsoft.Win32;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.Platform.ConfigFiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OSDiagTool.Platform {
    class PlatformUtils {

        public static string GetPlatformVersion(string osServerRegistry) {

            // Find Installation path and Platform Version 
            string osPlatformVersion = null;
            try {
                //FileLogger.TraceLog("Verifying OutSystems Platform Version...");
                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(osServerRegistry);

                osPlatformVersion = (string)OSPlatformInstaller.GetValue("Server");
                //FileLogger.TraceLog("Platform version: " + osPlatformVersion);

            } catch (Exception) {
                //FileLogger.LogError(" * Unable to find OutSystems Platform Server Installation... * ", e.Message + e.StackTrace);
                return null;
            }

            return osPlatformVersion;

        }

        public static string GetPlatformInstallationPath(string osServerRegistry) {

            try {
                //FileLogger.TraceLog("Finding OutSystems Platform Installation Path...");

                RegistryKey OSPlatformInstaller = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(osServerRegistry);
                string osInstallationFolder = (string)OSPlatformInstaller.GetValue("");

                return osInstallationFolder;

            } catch (Exception) {
                //FileLogger.LogError(" * Unable to find OutSystems Platform Version... * ", e.Message + e.StackTrace);
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

        public static Dictionary<string, string> GetServerList(string dbEngine, int queryTimeout, SqlConnection SqlConnection = null, OracleConnection orclConnection = null, string platformDBAdminUser = null)
        {

            Dictionary<string, string> lst = new Dictionary<string, string>();

            if (dbEngine.Equals("sqlserver"))
            {
                string _selectPlatSVCSObserver = "SELECT NAME, IP_ADDRESS FROM OSSYS_SERVER";

                SqlCommand cmd = new SqlCommand(_selectPlatSVCSObserver, SqlConnection)
                {
                    CommandTimeout = queryTimeout
                };

                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    //Loop through results
                    while (dataReader.Read())
                    {
                        lst.Add(Convert.ToString(dataReader["NAME"]), Convert.ToString(dataReader["IP_ADDRESS"]));
                    }
                }
            }
            else if (dbEngine.Equals("oracle"))
            {

                string _selectPlatSVCSObserver = "SELECT NAME, IP_ADDRESS FROM " + platformDBAdminUser + "." + "OSSYS_SERVER";

                OracleCommand cmd = new OracleCommand(_selectPlatSVCSObserver, orclConnection)
                {
                    CommandTimeout = queryTimeout
                };

                using (IDataReader dataReader = cmd.ExecuteReader())
                {
                    //Loop through results
                    while (dataReader.Read())
                    {
                        lst.Add(Convert.ToString(dataReader["NAME"]), Convert.ToString(dataReader["IP_ADDRESS"]));
                    }
                }
            }

            return lst;
        }

        public static string GetPlatformDBAdminUser() {

            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;
            
            return platformDBInfo.GetProperty("AdminUser").Value;

        }

        /*
         * Read configuration section from the server.hsconf file
         */
        public static string GetConfigurationValue(string element, ConfigFileInfo platformInfo)
        {
            // We need to check if the configuration exists, if not, return null
            try {
                return platformInfo.GetProperty(element).Value;
            } catch (Exception e) {
                FileLogger.LogError("Failed to retrieve platform configuration value " + element + " : ", e.Message + e.StackTrace);
                return null;
            }
        }
    }
}
