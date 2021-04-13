using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.Utils;

namespace OSDiagTool.Platform.Requirements
{
    class PlatformRequirements
    {
        public static void ValidateRequirements(string dbEngine, string reqFilePath, OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel sqlConnString = null, 
            DBConnector.OracleConnStringModel oracleConnString = null)
        {
            /* TODO: validate if we are:
             *  - Inside a pure Controller
             *  - Check the OutSystems service status
             */

            bool checkNetworkRequirements = false;
            string compilerServiceHostName;
            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            NetworkUtils check = new NetworkUtils();

            // Get current server's IP address
            string serverIP = check.PingAddress("");

            // Validate if localhost is resolving to 127.0.0.1
            string getLocalhostAddress = check.PingAddress("localhost"); 

            // Get the compiler service hostname
            compilerServiceHostName = Convert.ToString(Platform.PlatformUtils.GetConfigurationValue("CompilerServerHostname", confFileParser.ServiceConfigurationInfo));

            /* If the compilerServiceHostName is a Dns, then replace it with the IP address
             * This will also convert a localhost value to 127.0.0.1
             */
            if (Uri.CheckHostName(compilerServiceHostName) == UriHostNameType.Dns)
                compilerServiceHostName = check.PingAddress(compilerServiceHostName);

            // Getting the ports set in Configuration Tool (server.hsconf file)
            List<int> portArray = new List<int>{
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerPort", confFileParser.ServerConfigurationInfo)), // Default port 80
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerSecurePort", confFileParser.ServerConfigurationInfo)), // Default port 443
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("DeploymentServerPort", confFileParser.ServiceConfigurationInfo)), // Default port 12001
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("SchedulerServerPort", confFileParser.ServiceConfigurationInfo)) // Default port 12002
            };

            // We are inside a Controller server
            if (serverIP == compilerServiceHostName || compilerServiceHostName == "127.0.0.1")
                // Add the port to the list above
                portArray.Add(Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("CompilerServerPort", confFileParser.ServiceConfigurationInfo))); // Default port 12000

            // Getting the information that we need from the database
            if (dbEngine.Equals("sqlserver"))
            {
                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(sqlConnString);

                /* TODO - implement here any queries to the database
                using (connection)
                {

                }*/
            connector.SQLCloseConnection(connection);
            }
            else if (dbEngine.Equals("oracle"))
            {
                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(oracleConnString);

                string platformDBAdminUser = Platform.PlatformUtils.GetPlatformDBAdminUser();

                // Same operations as SQL Server
                /* TODO - implement here any queries to the database
                using (connection)
                {

                }*/
                connector.OracleCloseConnection(connection);
            }

            
            // Write the results to log file
            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(reqFilePath, "requirements.log"))))
            {
                writer.WriteLine("Platform Requeriments\n\n========== Validating Network Requirements ==========\n");

                // Inform the server IP
                writer.WriteLine(string.Format("{0}: [INFO] Retrieving IP address from this server..." +
                    "\n{0}: [INFO] IP address detected in this server: {1}.", DateTime.Now.ToString(), serverIP));
                writer.WriteLine(string.Format("{0}: [INFO] Retrieving compiler service host name from the Configuration Tool..." +
                    "\n{0}: [INFO] Compiler service host name detected in the Configuration Tool: {1}.", DateTime.Now.ToString(), compilerServiceHostName));

                /* Check if we are inside a Controller server
                 * The serverIP should be the same as the compilerServiceHostName or 
                 * The compilerServiceHostName should be localhost
                 */
                if (serverIP == compilerServiceHostName || compilerServiceHostName == "127.0.0.1")
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that this is a server with a Deployment Controller role.", DateTime.Now.ToString()));
                else
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that this is a server with a Front-end role.", DateTime.Now.ToString()));

                // Inform if localhost is resolving correctly
                writer.WriteLine(string.Format("\n{0}: [INFO] Performing a ping request to localhost...", DateTime.Now.ToString()));
                if (getLocalhostAddress == "127.0.0.1")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost resolves to {1}.", DateTime.Now.ToString(), getLocalhostAddress));
                else
                {
                    writer.WriteLine(string.Format("{0}: [ERROR] Localhost is resolving to {1} instead of 127.0.0.1.", DateTime.Now.ToString(), getLocalhostAddress));
                    checkNetworkRequirements = true;
                }
                // Localhost must be accessible by HTTP on 127.0.0.1
                writer.WriteLine(string.Format("{0}: [INFO] Localhost is returning the following response when using port {1}:\n{2}", 
                    DateTime.Now.ToString(), portArray[0], check.OpenTcpStream("localhost", portArray[0])));

                // Validate ports
                writer.WriteLine(string.Format("\n{0}: [INFO] Checking if the required ports are open for the IP {1}...", DateTime.Now.ToString(), serverIP));
                foreach (int port in portArray)
                {
                    // Check ports for the server IP
                    if (check.OpenTcpStream(serverIP, port) != null)
                        writer.WriteLine(string.Format("{0}: [INFO] The TCP port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, serverIP));
                    else {
                        writer.WriteLine(string.Format("{0}: [ERROR] Could not detect if port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, serverIP));
                        checkNetworkRequirements = true;
                    }
                }

                // Check OutSystems services
                writer.WriteLine(string.Format("\n{0}: [INFO] Checking if the status of the OutSystems services...", DateTime.Now.ToString()));
                writer.WriteLine(string.Format("{0}: [{1}] The status of the OutSystems Deployment Controller Service is: {2}", 
                    DateTime.Now.ToString(),
                    Utils.WinUtils.ServiceStatus("OutSystems Deployment Controller Service") == "Running" ? "INFO" : "ERROR",
                    Utils.WinUtils.ServiceStatus("OutSystems Deployment Controller Service")));

                // Warn the customer that he should review the Network Requirements of the Platform
                if (checkNetworkRequirements)
                    writer.WriteLine("\n[ERROR] Please review the OutSystems Network Requirements." +
                        "\nhttps://success.outsystems.com/Documentation/11/Setting_Up_OutSystems/OutSystems_network_requirements");

                writer.WriteLine(string.Format("\n========== Log ended at {0} ==========", DateTime.Now.ToString()));
            }

        }
    }
}
