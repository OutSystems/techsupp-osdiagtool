using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.Utils;

namespace OSDiagTool.Platform
{
    class PlatformDiagnostic
    {
        public static void WriteLog(string dbEngine, string reqFilePath, OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel sqlConnString = null,
            DBConnector.OracleConnStringModel oracleConnString = null)
        {
            /* TODO:
             *  Check if machineIP = compilerServiceIP if IsController is true
             *  Validate if we are in the OutSystems Cloud
             *  Validate if we are inside a pure Controller
             *  Validate if we are inside a LifeTime server
             *  Check RabbitMQ port
             *  Check conectivity to other FEs
             */

            bool checkNetworkRequirements = false;
            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            // Get current server's IP address
            string machineIP = Utils.NetworkUtils.PingAddress("");
            // Validate if localhost is resolving to 127.0.0.1
            string getLocalhostAddress = Utils.NetworkUtils.PingAddress("localhost");
            string compilerServiceIP = GetHostname(confFileParser, "CompilerServerHostname", true);
            string compilerServiceHostname = GetHostname(confFileParser, "CompilerServerHostname", false);
            string cacheServiceIP = GetHostname(confFileParser, "ServiceHost", true);
            string cacheServiceHostname = GetHostname(confFileParser, "ServiceHost", false);
            bool IsController = IsControllerServer(machineIP, compilerServiceIP);
            List<int> portArray = GetPortArray(confFileParser, IsController);
            List<string> osServices = GetOsServices(IsController);

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
            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(reqFilePath, "IP_" + machineIP + "_Diagnostic.log"))))
            {
                writer.WriteLine(string.Format("Platform Diagnostic{0}{0}========== Validating Network Requirements =========={0}", Environment.NewLine));

                // Inform the server IP and hostname
                writer.WriteLine(string.Format("{1}: [INFO] Retrieving IP address from this server..." +
                    "{0}{1}: [INFO] IP address detected in this server: {2}.", Environment.NewLine, DateTime.Now.ToString(), machineIP));
                writer.WriteLine(string.Format("{1}: [INFO] Retrieving compiler service hostname from the Configuration Tool..." +
                    "{0}{1}: [INFO] Compiler service hostname detected: {2}.", Environment.NewLine, DateTime.Now.ToString(), compilerServiceHostname));
                if (compilerServiceHostname != compilerServiceIP)
                    writer.WriteLine(string.Format("{0}: [INFO] Resolving compiler service hostname..." +
                        "{0}{1}: [INFO] Compiler service hostname resolves to the IP: {2}.", Environment.NewLine, DateTime.Now.ToString(), compilerServiceIP));

                // Inform if we are in a controller server
                if (IsController)
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that this is a server with a Deployment Controller role.", DateTime.Now.ToString()));
                else
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that this is a server with a Front-end role.", DateTime.Now.ToString()));

                // Inform the cache service IP and hostname
                writer.WriteLine(string.Format("{0}{1}: [INFO] Retrieving cache invalidation service hostname from the Configuration Tool..." +
                    "{0}{1}: [INFO] Cache invalidation service hostname detected: {2}.", Environment.NewLine, DateTime.Now.ToString(), cacheServiceHostname));
                if (cacheServiceHostname != cacheServiceIP)
                    writer.WriteLine(string.Format("{1}: [INFO] Resolving cache invalidation service hostname..." +
                        "{0}{1}: [INFO] Cache invalidation service hostname resolves to the IP: {2}.", Environment.NewLine, DateTime.Now.ToString(), cacheServiceIP));

                // Inform if localhost is resolving to 127.0.0.1
                writer.WriteLine(string.Format("{0}{1}: [INFO] Performing a ping request to localhost...", Environment.NewLine, DateTime.Now.ToString()));
                if (getLocalhostAddress == "127.0.0.1")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost resolves to {1}.", DateTime.Now.ToString(), getLocalhostAddress));
                else
                {
                    writer.WriteLine(string.Format("{0}: [ERROR] Localhost is resolving to {1} instead of 127.0.0.1.", DateTime.Now.ToString(), getLocalhostAddress));
                    checkNetworkRequirements = true;
                }

                // Localhost must be accessible by HTTP on 127.0.0.1
                if (Utils.NetworkUtils.OpenTcpStream("localhost", portArray[0]) == "200")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost is returning the following status code response when using port {1}: STATUS CODE {2}.",
                        DateTime.Now.ToString(), portArray[0], Utils.NetworkUtils.OpenTcpStream("localhost", portArray[0])));
                
                // OutSystems Cloud can return 302 in the Controller server
                else if (Utils.NetworkUtils.OpenTcpStream("localhost", portArray[0]) == "302")
                    writer.WriteLine(string.Format("{0}: [WARNING] Localhost is returning the following status code response when using port {1}: STATUS CODE {2}.",
                        DateTime.Now.ToString(), portArray[0], Utils.NetworkUtils.OpenTcpStream("localhost", portArray[0])));
                else
                {
                    writer.WriteLine(string.Format("{0}: [WARNING] Localhost is returning the following status code response when using port {1}: STATUS CODE {2}.",
                        DateTime.Now.ToString(), portArray[0], Utils.NetworkUtils.OpenTcpStream("localhost", portArray[0])));
                    checkNetworkRequirements = true;
                }

                // Validate ports
                writer.WriteLine(string.Format("{0}{1}: [INFO] Detecting ports set in the Configuration Tool..." +
                    "{0}{1}: [INFO] Ports detected: {2}", Environment.NewLine, DateTime.Now.ToString(), String.Join(", ", portArray)));
                writer.WriteLine(string.Format("{0}: [INFO] Checking if the ports are open for the IP {1}...", DateTime.Now.ToString(), machineIP));
                foreach (int port in portArray)
                {
                    // Check ports for the server IP
                    if (Utils.NetworkUtils.OpenTcpStream(machineIP, port) != null)
                        writer.WriteLine(string.Format("{0}: [INFO] The TCP port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, machineIP));
                    else {
                        writer.WriteLine(string.Format("{0}: [ERROR] Could not detect if port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, machineIP));
                        checkNetworkRequirements = true;
                    }
                }

                // Check OutSystems services status
                writer.WriteLine(string.Format("{0}{1}: [INFO] Checking if the status of the OutSystems services...", Environment.NewLine, DateTime.Now.ToString()));
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
                writer.WriteLine(string.Format("{0}{1}: [INFO] Examining the network interface of the server...", Environment.NewLine, DateTime.Now.ToString()));
                if (NetworkUtils.IsNetworkUp())
                    writer.WriteLine(string.Format("{0}: [INFO] The server returned that there are network connections available.", DateTime.Now.ToString()));
                else
                    writer.WriteLine(string.Format("{0}: [WARNING] Could not find any network connections available.", DateTime.Now.ToString()));

                // Warn the customer that he should review the Network Requirements of the Platform
                if (checkNetworkRequirements)
                    writer.WriteLine("{0}[ERROR] Please review the OutSystems Network Requirements." +
                        "{0}https://success.outsystems.com/Documentation/11/Setting_Up_OutSystems/OutSystems_network_requirements", Environment.NewLine);

                writer.WriteLine(string.Format("{0}========== Log ended at {1} ==========", Environment.NewLine, DateTime.Now.ToString()));
            }
        }

        // Getting the ports set in Configuration Tool (server.hsconf file)
        private static List<int> GetPortArray(ConfigFileReader confFileParser, bool IsController) {
            
            List<int> ports = new List<int> {
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerPort", confFileParser.ServerConfigurationInfo)), // Default port 80
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerSecurePort", confFileParser.ServerConfigurationInfo)), // Default port 443
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("DeploymentServerPort", confFileParser.ServiceConfigurationInfo)), // Default port 12001
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("SchedulerServerPort", confFileParser.ServiceConfigurationInfo)) // Default port 12002
            };

            // If we are inside a Controller server 
            if (IsController)
                // Add the compiler port to be validated
                ports.Add(Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("CompilerServerPort", confFileParser.ServiceConfigurationInfo))); // Default port 12000

            return ports;
        }

        // Getting OS services list
        private static List<string> GetOsServices(bool IsController)
        {
            // Setting the OutSystems Services names
            List<string> services = new List<string> {
                "OutSystems Deployment Service",
                "OutSystems Scheduler Service"
            };

            // If we are inside a Controller server 
            if (IsController)
                // Add the deployment controller service to be validated
                services.Add("OutSystems Deployment Controller Service");

            return services;
        }

        // Getting hostname from server.conf
        private static string GetHostname(ConfigFileReader confFileParser, string confValue, bool convertToIP)
        {
            String hostname = null;
            switch (confValue)
            {
                case "CompilerServerHostname": // Compiler service
                    hostname = Convert.ToString(Platform.PlatformUtils.GetConfigurationValue(confValue, confFileParser.ServiceConfigurationInfo));
                    break;
                case "ServiceHost": // Cache invalidation service
                    hostname = Convert.ToString(Platform.PlatformUtils.GetConfigurationValue(confValue, confFileParser.CacheConfigurationInfo));
                    break;
            }

            /* If the hostname is a Dns, then replace it with the IP address
             * This will also convert a localhost value to 127.0.0.1
             */
            if (Uri.CheckHostName(hostname) == UriHostNameType.Dns && convertToIP)
                hostname = Utils.NetworkUtils.PingAddress(hostname);

            return hostname;
        }

        // Check if we are inside a Controller server
        private static bool IsControllerServer(string IP, string compilerHostName)
        {
            /* The serverIP should be the same as the compilerServiceHostName or 
             * The compilerServiceHostName should be localhost
             */
            return IP == compilerHostName || compilerHostName == "127.0.0.1";
        }
    }
}
