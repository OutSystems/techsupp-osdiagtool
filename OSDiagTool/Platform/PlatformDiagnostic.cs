using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.Utils;

namespace OSDiagTool.Platform.Diagnostic
{
    class PlatformDiagnostic
    {
        public static void ValidateRequirements(string dbEngine, string reqFilePath, OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel sqlConnString = null, 
            DBConnector.OracleConnStringModel oracleConnString = null)
        {
            /* TODO:
             *  - Add date and time to file log name
             *  - Validate if we are inside a pure Controller
             *  - Check RabbitMQ port
             *  - Check conectivity to other FEs
             */

            bool checkNetworkRequirements = false;
            bool IsControllerServer;
            string compilerServiceHostName;
            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            NetworkUtils networkUtils = new NetworkUtils();

            // Get current server's IP address
            string serverIP = networkUtils.PingAddress("");

            // Validate if localhost is resolving to 127.0.0.1
            string getLocalhostAddress = networkUtils.PingAddress("localhost"); 

            // Get the compiler service hostname
            compilerServiceHostName = Convert.ToString(Platform.PlatformUtils.GetConfigurationValue("CompilerServerHostname", confFileParser.ServiceConfigurationInfo));

            /* If the compilerServiceHostName is a Dns, then replace it with the IP address
             * This will also convert a localhost value to 127.0.0.1
             */
            if (Uri.CheckHostName(compilerServiceHostName) == UriHostNameType.Dns)
                compilerServiceHostName = networkUtils.PingAddress(compilerServiceHostName);

            /* Check if we are inside a Controller server
             * The serverIP should be the same as the compilerServiceHostName or 
             * The compilerServiceHostName should be localhost
             */
            IsControllerServer = (serverIP == compilerServiceHostName || compilerServiceHostName == "127.0.0.1");

            // Getting the ports set in Configuration Tool (server.hsconf file)
            List<int> portArray = new List<int> {
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerPort", confFileParser.ServerConfigurationInfo)), // Default port 80
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerSecurePort", confFileParser.ServerConfigurationInfo)), // Default port 443
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("DeploymentServerPort", confFileParser.ServiceConfigurationInfo)), // Default port 12001
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("SchedulerServerPort", confFileParser.ServiceConfigurationInfo)) // Default port 12002
            };

            // Setting the OutSystems Services names
            List<string> osServices = new List<string> {
                "OutSystems Deployment Service",
                "OutSystems Scheduler Service"
            };

            // If we are inside a Controller server 
            if (IsControllerServer) {
                // Add the compiler port to be validated
                portArray.Add(Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("CompilerServerPort", confFileParser.ServiceConfigurationInfo))); // Default port 12000
                // Add the deployment controller service to be validated
                osServices.Add("OutSystems Deployment Controller Service");
            }

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
            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(reqFilePath, "PlatformDiagnostic.log"))))
            {
                writer.WriteLine("Platform Diagnostic\n\n========== Validating Network Requirements ==========\n");

                // Inform the server IP
                writer.WriteLine(string.Format("{0}: [INFO] Retrieving IP address from this server..." +
                    "\n{0}: [INFO] IP address detected in this server: {1}.", DateTime.Now.ToString(), serverIP));
                writer.WriteLine(string.Format("{0}: [INFO] Retrieving compiler service host name from the Configuration Tool..." +
                    "\n{0}: [INFO] Compiler service host name detected in the Configuration Tool: {1}.", DateTime.Now.ToString(), compilerServiceHostName));

                // Inform if we are in a controller server
                if (IsControllerServer)
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that this is a server with a Deployment Controller role.", DateTime.Now.ToString()));
                else {
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that this is a server with a Front-end role.", DateTime.Now.ToString()));
                    checkNetworkRequirements = true;
                }                    

                // Inform if localhost is resolving to 127.0.0.1
                writer.WriteLine(string.Format("\n{0}: [INFO] Performing a ping request to localhost...", DateTime.Now.ToString()));
                if (getLocalhostAddress == "127.0.0.1")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost resolves to {1}.", DateTime.Now.ToString(), getLocalhostAddress));
                else
                {
                    writer.WriteLine(string.Format("{0}: [ERROR] Localhost is resolving to {1} instead of 127.0.0.1.", DateTime.Now.ToString(), getLocalhostAddress));
                    checkNetworkRequirements = true;
                }

                // Localhost must be accessible by HTTP on 127.0.0.1
                if (networkUtils.OpenTcpStream("localhost", portArray[0]) == "200")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost is returning the following status code response when using port {1}: Status code {2}", 
                        DateTime.Now.ToString(), portArray[0], networkUtils.OpenTcpStream("localhost", portArray[0])));
                else
                {
                    writer.WriteLine(string.Format("{0}: [ERROR] Localhost is returning the following status code response when using port {1}: Status code {2}",
                        DateTime.Now.ToString(), portArray[0], networkUtils.OpenTcpStream("localhost", portArray[0])));
                    checkNetworkRequirements = true;
                }

                // Validate ports
                writer.WriteLine(string.Format("\n{0}: [INFO] Checking if the required ports are open for the IP {1}...", DateTime.Now.ToString(), serverIP));
                foreach (int port in portArray)
                {
                    // Check ports for the server IP
                    if (networkUtils.OpenTcpStream(serverIP, port) != null)
                        writer.WriteLine(string.Format("{0}: [INFO] The TCP port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, serverIP));
                    else {
                        writer.WriteLine(string.Format("{0}: [ERROR] Could not detect if port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, serverIP));
                        checkNetworkRequirements = true;
                    }
                }

                // Check OutSystems services status
                writer.WriteLine(string.Format("\n{0}: [INFO] Checking if the status of the OutSystems services...", DateTime.Now.ToString()));
                foreach (string service in osServices)
                {
                    // Check the status of OutSystems Services
                    if (Utils.WinUtils.ServiceStatus(service) == "Running")
                        writer.WriteLine(string.Format("{0}: [INFO] The status of the {1} is: {2}.", DateTime.Now.ToString(), service, Utils.WinUtils.ServiceStatus(service)));
                    else
                    {
                        writer.WriteLine(string.Format("{0}: [ERROR] The status of the {1} is: {2}.", DateTime.Now.ToString(), service, Utils.WinUtils.ServiceStatus(service)));
                        checkNetworkRequirements = true;
                    }
                }

                // Validating Network interface status
                writer.WriteLine(string.Format("\n{0}: [INFO] Examining the network interface of the server...", DateTime.Now.ToString()));
                if (NetworkUtils.IsNetworkUp())
                    writer.WriteLine(string.Format("{0}: [INFO] The server returned that there are network interfaces marked as 'up'.", DateTime.Now.ToString()));
                else
                    writer.WriteLine(string.Format("{0}: [WARNING] Could not find any network interface is marked as 'up'.", DateTime.Now.ToString()));

                // Warn the customer that he should review the Network Requirements of the Platform
                if (checkNetworkRequirements)
                    writer.WriteLine("\n[ERROR] Please review the OutSystems Network Requirements." +
                        "\nhttps://success.outsystems.com/Documentation/11/Setting_Up_OutSystems/OutSystems_network_requirements");

                writer.WriteLine(string.Format("\n========== Log ended at {0} ==========", DateTime.Now.ToString()));
            }

        }
    }
}
