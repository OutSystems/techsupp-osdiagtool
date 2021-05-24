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
        public static void WriteLog(string dbEngine, string reqFilePath, OSDiagToolConf.ConfModel.strConfModel configurations, 
            DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel oracleConnString = null)
        {
            /* TODO:
             *  Review connectivity test logic, considering that the HostnameToIP might not return an IP address
             *  Check conectivity to other FEs
             */

            // Getting the information that we need from the database
            bool IsLifeTimeEnvironment = false;

            if (dbEngine.Equals("sqlserver"))
            {
                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(sqlConnString);

                using (connection)
                {
                    IsLifeTimeEnvironment = Platform.PlatformUtils.IsLifeTimeEnvironment(dbEngine, configurations.queryTimeout, connection);
                }
                connector.SQLCloseConnection(connection);
            }
            else if (dbEngine.Equals("oracle"))
            {
                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(oracleConnString);
                string platformDBAdminUser = Platform.PlatformUtils.GetPlatformDBAdminUser();

                using (connection)
                {
                    IsLifeTimeEnvironment = Platform.PlatformUtils.IsLifeTimeEnvironment(dbEngine, configurations.queryTimeout, null, connection, platformDBAdminUser);
                }
                connector.OracleCloseConnection(connection);
            }

            // Setting variables
            bool checkNetworkRequirements = false;
            bool checkOutSystemsServices = false;

            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            // Getting list of ports from server.conf
            List<int> portArray = GetPortArray(confFileParser);
            Dictionary<string, string> osServices = GetOsServices();
            // Get current server's IP address
            string machineIP = Utils.NetworkUtils.PingAddress("");
            // Validate if localhost is resolving to 127.0.0.1
            string getLocalhostAddress = Utils.NetworkUtils.PingAddress("localhost");
            string compilerServiceHostname = GetHostname(confFileParser, "CompilerServerHostname");
            string cacheServiceHostname = GetHostname(confFileParser, "ServiceHost");
            string compilerConnectionResult = TryGetIP(compilerServiceHostname, portArray[0]); 
            string cacheConnectionResult = TryGetIP(cacheServiceHostname, portArray[5]);
            string serverRole = GetServerRole(osServices);            

            // Write the results to log file
            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(reqFilePath, "IP_" + machineIP + "_Diagnostic.log"))))
            {
                writer.WriteLine(string.Format("== Platform Diagnostic =={0}{0}========== Validating Network Requirements =========={0}", Environment.NewLine));

                // --- Server tests ---

                // Inform the server IP
                writer.WriteLine(string.Format("{1}: [INFO] Retrieving IP address from this server..." +
                    "{0}{1}: [INFO] IP address detected in this server: {2}.", Environment.NewLine, DateTime.Now.ToString(), machineIP));

                // Inform if localhost is resolving to 127.0.0.1
                writer.WriteLine(string.Format("{0}: [INFO] Performing a ping request to localhost...",DateTime.Now.ToString()));
                if (getLocalhostAddress == "127.0.0.1")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost resolves to {1}.", DateTime.Now.ToString(), getLocalhostAddress));
                else
                {
                    writer.WriteLine(string.Format("{0}: [ERROR] Localhost is resolving to {1} instead of 127.0.0.1.", DateTime.Now.ToString(), getLocalhostAddress));
                    checkNetworkRequirements = true;
                }

                // Localhost must be accessible by HTTP on 127.0.0.1
                if (Utils.NetworkUtils.OpenTcpStream(getLocalhostAddress, portArray[0]) == "Status code 200" ||
                    Utils.NetworkUtils.OpenTcpStream(getLocalhostAddress, portArray[0]) == "Status code 302")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost is returning the following response when connecting using port {1}: {2}.",
                        DateTime.Now.ToString(), portArray[0], Utils.NetworkUtils.OpenTcpStream(getLocalhostAddress, portArray[0])));
                else
                {
                    writer.WriteLine(string.Format("{0}: [ERROR] Localhost is returning the following response when connecting using port {1}: {2}.",
                        DateTime.Now.ToString(), portArray[0], Utils.NetworkUtils.OpenTcpStream(getLocalhostAddress, portArray[0])));
                    checkNetworkRequirements = true;
                }

                // --- Controller tests ---

                // Inform the Controller hostname
                writer.WriteLine(string.Format("{0}{1}: [INFO] Retrieving Controller hostname from the Configuration Tool..." +
                    "{0}{1}: [INFO] Detected the following Controller hostname: {2}.", Environment.NewLine, DateTime.Now.ToString(), compilerServiceHostname));
                
                // If compiler hostname is not an IP, then inform connection results
                if (!IsIP(compilerServiceHostname))
                    writer.WriteLine(string.Format("{1}: [INFO] Trying to connect to the Controller hostname to resolve the IP address..." +
                        "{0}{1}: [INFO] Controller hostname returned the following response: {2}.", Environment.NewLine, DateTime.Now.ToString(), compilerConnectionResult));

                // --- OutSystems Service tests ---

                // Check OutSystems services status
                writer.WriteLine(string.Format("{0}{1}: [INFO] Checking if the status of the OutSystems services...", Environment.NewLine, DateTime.Now.ToString()));
                foreach (var service in osServices)
                {
                    // Check the status of OutSystems Services
                    if (service.Value == "Running")
                        writer.WriteLine(string.Format("{0}: [INFO] The status of the {1} is: {2}.", DateTime.Now.ToString(), service.Key, service.Value));
                    else
                    {
                        writer.WriteLine(string.Format("{0}: [ERROR] The status of the {1} is: {2}.", DateTime.Now.ToString(), service.Key, service.Value));
                        checkNetworkRequirements = true;
                    }
                }

                // Inform the role of the server
                if (serverRole == "Inconsistent state")
                {
                    writer.WriteLine(string.Format("{0}: [ERROR] Could not detect the server role - the OutSystems services seems to be in an inconsistent state.", DateTime.Now.ToString()));
                    checkOutSystemsServices = true;
                }
                else
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that the role of this is a server is a {1}.", DateTime.Now.ToString(), serverRole));

                // Inform if we can detect LifeTime
                if (IsLifeTimeEnvironment)
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that this server has LifeTime installed.", DateTime.Now.ToString()));

                // --- Cache Invalidation Service tests ---

                // Inform the cache service IP and hostname
                writer.WriteLine(string.Format("{0}{1}: [INFO] Retrieving cache invalidation service hostname from the Configuration Tool..." +
                    "{0}{1}: [INFO] Detected the following Cache invalidation service hostname: {2}.", Environment.NewLine, DateTime.Now.ToString(), cacheServiceHostname));

                // If cache service hostname is not an IP, then inform connection results
                if (!IsIP(cacheServiceHostname))
                    writer.WriteLine(string.Format("{1}: [INFO] Trying to connect to the cache invalidation service hostname to resolve the IP address..." +
                        "{0}{1}: [INFO] Cache invalidation service hostname returned the following response: {2}.", Environment.NewLine, DateTime.Now.ToString(), cacheConnectionResult));
                
                // Inform if we detect that the cache invalidation service is installed
                if (compilerConnectionResult == cacheConnectionResult || compilerServiceHostname == cacheServiceHostname)
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that the cache invalidation service is installed in the Controller server.", DateTime.Now.ToString()));

                // --- Connectivity tests ---

                // Validate connectivity requirements
                writer.WriteLine(string.Format("{0}{1}: [INFO] Detecting ports configured in the Configuration Tool..." +
                    "{0}{1}: [INFO] Ports detected: {2}", Environment.NewLine, DateTime.Now.ToString(), String.Join(", ", portArray)));
                writer.WriteLine(string.Format("{0}: [INFO] Performing connectivity tests...", DateTime.Now.ToString()));

                string response, ipNumber;
                foreach (int port in portArray)
                {
                    if (port == portArray[4]) { // Deployment controller port and IP

                        response = Utils.NetworkUtils.OpenTcpStream(compilerConnectionResult, port); 
                        ipNumber = compilerConnectionResult;
                    } else if (port == portArray[5]) { // RabbitMQ port and IP

                        response = Utils.NetworkUtils.OpenTcpStream(cacheConnectionResult, port);
                        ipNumber = cacheConnectionResult;
                    } else {
                        response = Utils.NetworkUtils.OpenTcpStream(machineIP, port);
                        ipNumber = machineIP;
                    }
                    writer.WriteLine(string.Format("{0}: [INFO] Trying to connect to {1} and TCP port {2}...",
                            DateTime.Now.ToString(), ipNumber, port));

                    if (response == null) {
                        writer.WriteLine(string.Format("{0}: [ERROR] Could not connect to {1} and TCP port {2} - Check the 'ConsoleLog' file for details.", 
                            DateTime.Now.ToString(), ipNumber, port));
                        checkNetworkRequirements = true;
                    }
                    else
                        writer.WriteLine(string.Format("{0}: [INFO] Connected to {1}, TCP port {2} - Response: {3}.",
                            DateTime.Now.ToString(), ipNumber, port, response));
                }

                // Validating Network interface status
                writer.WriteLine(string.Format("{0}{1}: [INFO] Examining the network interface of the server...", Environment.NewLine, DateTime.Now.ToString()));
                if (NetworkUtils.IsNetworkUp())
                    writer.WriteLine(string.Format("{0}: [INFO] The server returned that there are network connections available.", DateTime.Now.ToString()));
                else
                    writer.WriteLine(string.Format("{0}: [WARNING] Could not find any network connections available.", DateTime.Now.ToString()));

                // --- Write errors ---

                // Warn the customer that he should review the Network Requirements of the Platform
                if (checkNetworkRequirements)
                    writer.WriteLine("{0}[ERROR] Please review the OutSystems Network Requirements:" +
                        "{0}https://success.outsystems.com/Documentation/11/Setting_Up_OutSystems/OutSystems_network_requirements", Environment.NewLine);

                if (checkOutSystemsServices)
                    writer.WriteLine("{0}[ERROR] Please review the documentation below in order to troubleshoot the OutSystems Services:" +
                        "{0}https://success.outsystems.com/Support/Enterprise_Customers/Troubleshooting/Troubleshooting_the_OutSystems_Platform_Server", Environment.NewLine);
                
                writer.WriteLine(string.Format("{0}========== Log ended at {1} ==========", Environment.NewLine, DateTime.Now.ToString()));
            }
        }

        // Getting the ports set in Configuration Tool (server.hsconf file)
        private static List<int> GetPortArray(ConfigFileReader confFileParser) {
            List<int> ports = new List<int> {
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerPort", confFileParser.ServerConfigurationInfo)), // Default port 80
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerSecurePort", confFileParser.ServerConfigurationInfo)), // Default port 443
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("DeploymentServerPort", confFileParser.ServiceConfigurationInfo)), // Deployment Service - Default value: 12001
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("SchedulerServerPort", confFileParser.ServiceConfigurationInfo)), // Scheduler Service  - Default value: 12002
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("CompilerServerPort", confFileParser.ServiceConfigurationInfo)), // Deployment Controller Service - Default value: 12000
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ServicePort", confFileParser.CacheConfigurationInfo)) // RabbitMQ port - Default value: 5672
            };
            return ports;
        }

        // Getting OS services list
        private static Dictionary<string, string> GetOsServices()
        {
            // Setting the OutSystems Services names
            var osServices = new Dictionary<string, string>
            {
                { "OutSystems Deployment Service", "" },
                { "OutSystems Scheduler Service", "" },
                { "OutSystems Deployment Controller Service", "" },
            };

            // Get OS service current status
            var keys = new List<string>(osServices.Keys);
            foreach (string key in keys)
            {
                osServices[key] = Utils.WinUtils.ServiceStatus(key);
            }
            return osServices;
        }

        // Getting hostname from server.conf
        private static string GetHostname(ConfigFileReader confFileParser, string confValue)
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
            return hostname;
        }

        // Converting the hostname to an IP address
        private static string TryGetIP(string hostname, int port)
        {
            // If the hostname passed is in the DNS format, then try to get the IP address
            if (Uri.CheckHostName(hostname) == UriHostNameType.Dns)
                return NetworkUtils.OpenTcpStream(hostname, port, true);

            // If the hostname is already an IP, then return it
            return hostname;
        }

        // Understand if we are inside a standalone Controller, Controller + Front-End or Front-End
        private static string GetServerRole(Dictionary<string, string> osServices)
        {
            if (osServices["OutSystems Deployment Controller Service"] == "Running"
                && osServices["OutSystems Deployment Service"] != "Running"
                && osServices["OutSystems Scheduler Service"] != "Running")

                return "Standalone Deployment Controller";

            else if (osServices["OutSystems Deployment Controller Service"] == "Running"
                && osServices["OutSystems Deployment Service"] == "Running"
                && osServices["OutSystems Scheduler Service"] == "Running")

                return "Deployment Controller sharing a server with a Front-end";

            else if (osServices["OutSystems Deployment Controller Service"] != "Running"
                && osServices["OutSystems Deployment Service"] == "Running"
                && osServices["OutSystems Scheduler Service"] == "Running")

                return "Front-End";

            // Everything else means that the OS services are facing a problem
            else
                return "Inconsistent state";
        }

        // Check if a hostname is an IP or not
        private static bool IsIP(string hostname)
        {
            return (Uri.CheckHostName(hostname) == UriHostNameType.IPv4 || Uri.CheckHostName(hostname) == UriHostNameType.IPv6);
        }

    }
}
