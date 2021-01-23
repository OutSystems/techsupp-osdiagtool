using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDiagTool.OSDiagToolConf;

namespace OSDiagTool {
    class OSDGTCmdLine {

        public static void CmdLineRun(OSDiagToolConf.ConfModel.strConfModel configurations, string dbms, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null,
            string dbSaUser = null, string dbSaPwd = null) {

            // Get Platform and Server files
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_platform][OSDiagToolConfReader._l3_platformAndServerConfigFiles]) {

                Program.GetPlatformAndServerFiles();

            }

            // Export Event Viewer and Server logs
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_serverLogs][OSDiagToolConfReader._l3_evtAndServer]) {

                    Program.ExportEventViewerAndServerLogs();
            }


            // Copy IIS Access logs
            if(configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_serverLogs][OSDiagToolConfReader._l3_iisLogs]) {

                    Program.CopyIISAccessLogs(configurations.IISLogsNrDays);

            }

            // Database Troubleshoot
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_databaseOperations][OSDiagToolConfReader._l3_databaseTroubleshoot]) {

                    Platform.PlatformConnectionStringDefiner ConnectionStringDefiner = new Platform.PlatformConnectionStringDefiner();
                    Platform.PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, false, true, ConnectionStringDefiner, dbSaUser, dbSaPwd);

                    if (Program.dbEngine.Equals("sqlserver")) {
                        Program.DatabaseTroubleshootProgram(configurations, ConnStringHelper.SQLConnString);

                    } else if (Program.dbEngine.Equals("oracle")) {
                        Program.DatabaseTroubleshootProgram(configurations, null, ConnStringHelper.OracleConnString);

                }
            }


            // IIS Thread dumps
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_threadDumps][OSDiagToolConfReader._l3_iisW3wp]) {

                Program.CollectThreadDumpsProgram(true, false);

            }

            // OutSystems Services Thread dumps 
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_threadDumps][OSDiagToolConfReader._l3_osServices]) {

                Program.CollectThreadDumpsProgram(false, true);

            }

            // IIS Memory Dumps 
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_memoryDumps][OSDiagToolConfReader._l3_iisW3wp]) {

                Program.CollectMemoryDumpsProgram(true, false);

            }

            // OutSystems Services Memory Dumps
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_memoryDumps][OSDiagToolConfReader._l3_osServices]) {

                Program.CollectMemoryDumpsProgram(false, true);

            }

            Program.GenerateZipFile();


        }


    }
}
