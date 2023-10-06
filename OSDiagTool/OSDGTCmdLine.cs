using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDiagTool.OSDiagToolConf;
using System.Diagnostics;
using System.Threading;



namespace OSDiagTool {
    class OSDGTCmdLine {
        // Cmd Line run does not export Platform Logs neither Metamodel

        public static string osDiagToolEventSource = "OSDiagTool";

        

        public static void CmdLineRun(OSDiagToolConf.ConfModel.strConfModel configurations, string dbms, DBConnector.SQLConnStringModel SQLConnectionString = null, DBConnector.OracleConnStringModel OracleConnectionString = null,
            string dbSaUser = null, string dbSaPwd = null) {

            Console.WriteLine("Starting OSDiagTool data collection.");
            EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool started", EventLogEntryType.Information);

            // Get Platform and Server files
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_platform][OSDiagToolConfReader._l3_platformAndServerConfigFiles]) {

                EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Fetching Platfrom and Server files", EventLogEntryType.Information);
                Program.GetPlatformAndServerFiles();

            }

            // Export Event Viewer and Server logs
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_serverLogs][OSDiagToolConfReader._l3_evtAndServer]) {

                EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Exporting Event Viewer and Server logs", EventLogEntryType.Information);
                Program.ExportEventViewerAndServerLogs();
            }


            // Copy IIS Access logs
            if(configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_serverLogs][OSDiagToolConfReader._l3_iisLogs]) {

                EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Exporting IIS Access logs (number of days exported: )" + configurations.IISLogsNrDays, EventLogEntryType.Information);
                Program.CopyIISAccessLogs(configurations.IISLogsNrDays);

            }

            // Database Troubleshoot
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_databaseOperations][OSDiagToolConfReader._l3_databaseTroubleshoot]) {

                Platform.PlatformConnectionStringDefiner ConnectionStringDefiner = new Platform.PlatformConnectionStringDefiner();
                Platform.PlatformConnectionStringDefiner ConnStringHelper = ConnectionStringDefiner.GetConnectionString(Program.dbEngine, false, true, ConnectionStringDefiner, dbSaUser, dbSaPwd);

                if (Program.dbEngine.Equals("sqlserver")) {
                    EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Performing Database Troubleshoot (Database engine: SQL Server);", EventLogEntryType.Information); 
                    Program.DatabaseTroubleshootProgram(configurations, ConnStringHelper.SQLConnString);

                } else if (Program.dbEngine.Equals("oracle")) {
                    EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Performing Database Troubleshoot (Database engine: Oracle);", EventLogEntryType.Information);
                    Program.DatabaseTroubleshootProgram(configurations, null, ConnStringHelper.OracleConnString);

                }
            }


            // IIS Thread dumps
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_threadDumps][OSDiagToolConfReader._l3_iisW3wp]) {

                EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Retrieving IIS Thread dumps;", EventLogEntryType.Information);
                Program.CollectThreadDumpsProgram(true, false);

            }

            // OutSystems Services Thread dumps 
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_threadDumps][OSDiagToolConfReader._l3_osServices]) {

                EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Retrieving OutSystems Services Thread dumps;", EventLogEntryType.Information);
                Program.CollectThreadDumpsProgram(false, true);

            }

            // IIS Memory Dumps 
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_memoryDumps][OSDiagToolConfReader._l3_iisW3wp]) {

                EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Retrieving IIS memory dumps;", EventLogEntryType.Information);
                Program.CollectMemoryDumpsProgram(true, false);

            }

            // OutSystems Services Memory Dumps
            if (configurations.osDiagToolConfigurations[OSDiagToolConfReader._l2_memoryDumps][OSDiagToolConfReader._l3_osServices]) {

                EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool: Retrieving OutSystems Services memory dumps;", EventLogEntryType.Information);
                Program.CollectMemoryDumpsProgram(false, true);

            }

            Program.GenerateZipFile();

            EventLog.WriteEntry(osDiagToolEventSource, "Command Line run of OSDiagTool finished", EventLogEntryType.Information);


        }


    }
}
