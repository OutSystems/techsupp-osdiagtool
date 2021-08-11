using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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

            // Getting the information that we need from the database
            bool IsLifeTimeEnvironment = false;
            Dictionary<string, string> databaseServerList = null;
            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);

            if (dbEngine.Equals("sqlserver"))
            {
                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(sqlConnString);

                using (connection)
                {
                    IsLifeTimeEnvironment = Platform.PlatformUtils.IsLifeTimeEnvironment(dbEngine, configurations.queryTimeout, connection);
                    databaseServerList = Platform.PlatformUtils.GetServerList(dbEngine, configurations.queryTimeout, connection);
                }
                connector.SQLCloseConnection(connection);
            }
            else if (dbEngine.Equals("oracle"))
            {
                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(oracleConnString);
                string platformDBAdminUser = Platform.PlatformUtils.GetConfigurationValue("AdminUser", confFileParser.DBPlatformInfo);

                using (connection)
                {
                    IsLifeTimeEnvironment = Platform.PlatformUtils.IsLifeTimeEnvironment(dbEngine, configurations.queryTimeout, null, connection, platformDBAdminUser);
                    databaseServerList = Platform.PlatformUtils.GetServerList(dbEngine, configurations.queryTimeout, null, connection, platformDBAdminUser);
                }
                connector.OracleCloseConnection(connection);
            }

            // Setting variables
            bool checkNetworkRequirements = false;
            bool checkOutSystemsServices = false;
            int step = 1;

            TimeZone localZone = TimeZone.CurrentTimeZone;
            // Getting list of ports from server.conf
            List<int> portList = GetPortList(confFileParser);
            Dictionary<string, string> osServices = GetOsServices();
            // Get current server's IP address
            string machineIP = Utils.NetworkUtils.PingAddress("");
            // Validate if localhost is resolving to 127.0.0.1
            string getLocalhostAddress = Utils.NetworkUtils.PingAddress("localhost");

            string compilerServiceHostname = GetHostname(confFileParser, "CompilerServerHostname");
            string cacheServiceHostname = GetHostname(confFileParser, "ServiceHost");
            string compilerConnectionResult = ResolveIP(compilerServiceHostname, portList[0]); 
            string cacheConnectionResult = ResolveIP(cacheServiceHostname, portList[5]);
            string serverRole = GetServerRole(osServices);
            List<ConnectionList> connectionTestList = GetConnectionList(portList, databaseServerList, machineIP, serverRole, 
                cacheServiceHostname, compilerServiceHostname, IsLifeTimeEnvironment);

            // Write the results to log file
            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(reqFilePath, "IP_" + machineIP + "_Network_Requirements.log"))))
            {
                writer.WriteLine(string.Format("== Platform Diagnostic =={0}{0}========== Validating Network Requirements =========={0}", Environment.NewLine));

                // --- Server tests ---
                writer.WriteLine(string.Format("{0}: [{1}] Checking this server...",  DateTime.Now.ToString(), step));

                // Inform server's timezone name
                writer.WriteLine(string.Format("{0}{1}: [{2}] Server's timezone: {3}.", Environment.NewLine, DateTime.Now.ToString(), step, localZone.StandardName));

                // Inform the server IP
                writer.WriteLine(string.Format("{0}{1}: [{2}] Retrieving IP address from this server..." +
                    "{0}{1}: [{2}] IP address detected in this server: {3}.", Environment.NewLine, DateTime.Now.ToString(), step, machineIP));

                // Inform if localhost is resolving to 127.0.0.1
                writer.WriteLine(string.Format("{0}: [{1}] Performing a ping request to localhost...", DateTime.Now.ToString(), step));
                if (getLocalhostAddress == "127.0.0.1")
                    writer.WriteLine(string.Format("{0}: [{1}] Localhost resolves to {2}.", DateTime.Now.ToString(), step, getLocalhostAddress));
                else
                {
                    writer.WriteLine(string.Format("{0}: [{1}] [WARNING] Localhost is resolving to {2} instead of 127.0.0.1.", DateTime.Now.ToString(), step, getLocalhostAddress));
                    checkNetworkRequirements = true;
                }

                // Localhost must be accessible by HTTP on 127.0.0.1
                string responseLocalhost = Utils.NetworkUtils.OpenWebRequest(getLocalhostAddress, portList[0]);
                // Log 4XX and 5XX responses
                if (responseLocalhost.StartsWith("4") || responseLocalhost.StartsWith("5"))
                {
                    writer.WriteLine(string.Format("{0}: [{1}] [WARNING] Localhost is returning the following response when connecting using port {2}: {3}.",
                        DateTime.Now.ToString(), step, portList[0], responseLocalhost));
                    checkNetworkRequirements = true;
                }
                else
                    writer.WriteLine(string.Format("{0}: [{1}] Localhost returned the following response when connecting using port {2}: {3}.",
                        DateTime.Now.ToString(), step, portList[0], responseLocalhost));

                // --- Controller tests ---
                step++;
                writer.WriteLine(string.Format("{0}{1}: [{2}] Checking the Controller server...", Environment.NewLine, DateTime.Now.ToString(), step));

                // Inform the Controller hostname
                writer.WriteLine(string.Format("{0}{1}: [{2}] Retrieving Controller hostname from the Configuration Tool..." +
                    "{0}{1}: [{2}] Detected the following Controller hostname: {3}.", Environment.NewLine, DateTime.Now.ToString(), step, compilerServiceHostname));
                
                // If compiler hostname is not an IP, then inform connection results
                if (!Utils.NetworkUtils.IsIP(compilerServiceHostname))
                    writer.WriteLine(string.Format("{1}: [{2}] Trying to connect to the Controller hostname to resolve the IP address..." +
                        "{0}{1}: [{2}] Controller hostname returned the following response: {3}.", Environment.NewLine, DateTime.Now.ToString(), step, compilerConnectionResult));

                // --- OutSystems Service tests ---
                step++;
                writer.WriteLine(string.Format("{0}{1}: [{2}] Checking the OutSystems services...", Environment.NewLine, DateTime.Now.ToString(), step));
                foreach (var service in osServices)
                {
                    // Check the status of OutSystems Services
                    writer.WriteLine(string.Format("{0}: [{1}] The status of the {2} is: {3}.", DateTime.Now.ToString(), step, service.Key, service.Value));
                }

                // Inform the role of the server
                if (serverRole == "Unknown role")
                {
                    writer.WriteLine(string.Format("{0}: [{1}] [WARNING] Could not detect the server role - the OutSystems services might be in an inconsistent state.", 
                        DateTime.Now.ToString(), step));
                    checkOutSystemsServices = true;
                }
                else
                    writer.WriteLine(string.Format("{0}: [{1}] Detected that this server has the role of a {2}.", DateTime.Now.ToString(), step, serverRole));

                // Inform if we can detect LifeTime
                if (IsLifeTimeEnvironment)
                    writer.WriteLine(string.Format("{0}: [{1}] Detected that this server has LifeTime installed.", DateTime.Now.ToString(), step));

                // --- Cache Invalidation Service tests ---
                step++;
                writer.WriteLine(string.Format("{0}{1}: [{2}] Checking the cache invalidation service...", Environment.NewLine, DateTime.Now.ToString(), step));

                // Inform the cache service IP and hostname
                writer.WriteLine(string.Format("{0}{1}: [{2}] Retrieving cache invalidation service hostname from the Configuration Tool..." +
                    "{0}{1}: [{2}] Detected the following cache invalidation service hostname: {3}.", Environment.NewLine, DateTime.Now.ToString(), step, cacheServiceHostname));

                // If cache service hostname is not an IP, then inform connection results
                if (!Utils.NetworkUtils.IsIP(cacheServiceHostname))
                    writer.WriteLine(string.Format("{1}: [{2}] Trying to connect to the cache invalidation service hostname to resolve the IP address..." +
                        "{0}{1}: [{2}] Cache invalidation service hostname returned the following response when connecting using port {3}: {4}.", 
                        Environment.NewLine, DateTime.Now.ToString(), step, portList[0], cacheConnectionResult));
                
                // Inform if we detect that the cache invalidation service is installed
                if (compilerConnectionResult == cacheConnectionResult || compilerServiceHostname == cacheServiceHostname)
                    writer.WriteLine(string.Format("{0}: [{1}] Detected that the cache invalidation service is installed in the Controller server.", DateTime.Now.ToString(), step));

                // --- Connectivity tests ---
                step++;
                writer.WriteLine(string.Format("{0}{1}: [{2}] Performing connectivity tests...", Environment.NewLine, DateTime.Now.ToString(), step));

                // Validate connectivity requirements
                writer.WriteLine(string.Format("{0}{1}: [{2}] Detecting ports configured in the Configuration Tool..." +
                    "{0}{1}: [{2}] Ports detected: {3}", Environment.NewLine, DateTime.Now.ToString(), step, String.Join(", ", portList)));

                string response = string.Empty; 
                for (int index = 0; index < connectionTestList.Count; index++)
                {
                    step++;
                    // Inform if there is something left to test from OSSYS_SERVER
                    if (index == 1 && connectionTestList[index].Name != "cache invalidation hostname set in the Configuration Tool")
                        writer.WriteLine(string.Format("{0}{1}: [{2}] Accessing the OSSYS_SERVER table...",
                            Environment.NewLine, DateTime.Now.ToString(), step));

                    writer.WriteLine(string.Format("{0}{1}: [{2}] Trying to connect to {3}...",
                                Environment.NewLine, DateTime.Now.ToString(), step, connectionTestList[index].Name));

                    foreach (int port in connectionTestList[index].Ports)
                    {
                        // Perform a web request for the HTTP and HTTPS ports
                        if (port == portList[0] || port == portList[1])
                            response = Utils.NetworkUtils.OpenWebRequest(connectionTestList[index].Hostname, port);
                        // Validate the ports for the rest
                        else
                            response = Utils.NetworkUtils.ConnectToPort(connectionTestList[index].Hostname, port);

                        if (response == null)
                        {
                            writer.WriteLine(string.Format("{0}: [{1}] [WARNING] Could not stabilish a connection to {2} using TCP port {3} - Check the 'ConsoleLog' file for details.",
                                DateTime.Now.ToString(), step, connectionTestList[index].Hostname, port));
                            checkNetworkRequirements = true;
                        }
                        // Log 4XX and 5XX responses
                        else if (response.StartsWith("4") || response.StartsWith("5"))
                        {
                            writer.WriteLine(string.Format("{0}: [{1}] [WARNING] Connected to {2} using TCP port {3}, however, the response was {4}.",
                                DateTime.Now.ToString(), step, connectionTestList[index].Hostname, port, response));
                            checkNetworkRequirements = true;
                        }
                        else
                            writer.WriteLine(string.Format("{0}: [{1}] Connected to {2} using TCP port {3} - Response: {4}.",
                                DateTime.Now.ToString(), step, connectionTestList[index].Hostname, port, response));
                    }
                }

                // --- Write warning log ---

                // Warn that you should review the Network Requirements of the Platform
                if (checkNetworkRequirements)
                    writer.WriteLine("{0}{1}: [WARNING] Please review the OutSystems Network Requirements:" +
                        "{0}https://success.outsystems.com/Documentation/11/Setting_Up_OutSystems/OutSystems_network_requirements", Environment.NewLine, DateTime.Now.ToString());

                if (checkOutSystemsServices)
                    writer.WriteLine("{0}{1}: [WARNING] Please review the documentation below in order to troubleshoot the OutSystems Services:" +
                        "{0}https://success.outsystems.com/Support/Enterprise_Customers/Troubleshooting/Troubleshooting_the_OutSystems_Platform_Server", Environment.NewLine, DateTime.Now.ToString());
                
                writer.WriteLine(string.Format("{0}========== Log ended at {1} ==========", Environment.NewLine, DateTime.Now.ToString()));
            }
        }

        // Getting OS services list
        private static Dictionary<string, string> GetOsServices()
        {
            // Setting the OutSystems Services names
            Dictionary<string, string> osServices = new Dictionary<string, string>
            {
                { "OutSystems Deployment Service", "" },
                { "OutSystems Scheduler Service", "" },
                { "OutSystems Deployment Controller Service", "" },
            };

            // Get OS service current status
            List<string> keys = new List<string>(osServices.Keys);
            foreach (string key in keys)
            {
                osServices[key] = Utils.WinUtils.ServiceStatus(key);
            }
            return osServices;
        }

        // Getting hostname from server.conf
        private static string GetHostname(ConfigFileReader confFileParser, string confValue)
        {
            String hostname = string.Empty;
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
        private static string ResolveIP(string hostname, int port)
        {
            // If the hostname passed is in the DNS format, then try to get the IP address
            if (Uri.CheckHostName(hostname) == UriHostNameType.Dns)
                return NetworkUtils.ConnectToPort(hostname, port, true);

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

                return "Deployment Controller with a Front-end";

            else if (osServices["OutSystems Deployment Controller Service"] != "Running"
                && osServices["OutSystems Deployment Service"] == "Running"
                && osServices["OutSystems Scheduler Service"] == "Running")

                return "Front-End";

            // Everything else means that the OS services are facing a problem
            else
                return "Unknown role";
        }

        // Getting the ports set in Configuration Tool (from the server.hsconf file)
        private static List<int> GetPortList(ConfigFileReader confFileParser)
        {
            List<int> ports = new List<int> {
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerPort", confFileParser.ServerConfigurationInfo)), // Index [0] - Default HTTP port
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ApplicationServerSecurePort", confFileParser.ServerConfigurationInfo)), // Index [1] - Default HTTPS port
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("DeploymentServerPort", confFileParser.ServiceConfigurationInfo)), // Index [2] - Deployment Service - Default value: 12001
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("SchedulerServerPort", confFileParser.ServiceConfigurationInfo)), // Index [3] - Scheduler Service  - Default value: 12002
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("CompilerServerPort", confFileParser.ServiceConfigurationInfo)), // Index [4] - Deployment Controller Service - Default value: 12000
                Int32.Parse(Platform.PlatformUtils.GetConfigurationValue("ServicePort", confFileParser.CacheConfigurationInfo)) // Index [5] - RabbitMQ port - Default value: 5672
            };
            return ports;
        }

        // Get the list used to test the connections between servers
        private static List<ConnectionList> GetConnectionList(List<int> ports, Dictionary<string, string> serverList, string machineIP, string serverRole, 
        string cacheServiceHostname, string compilerServiceHostname, bool IsLifeTimeEnvironment)
        {
            List<ConnectionList> testList = new List<ConnectionList>();

            // Set up connectivity tests to machine IP
            switch (serverRole)
            {
                case "Standalone Deployment Controller":
                    testList.Add(new ConnectionList { Name = "this server's IP, with the role of a " + serverRole, Hostname = machineIP, Ports = new List<int>() { 
                        ports[0], ports[1], ports[4] 
                    } });
                    break;
                case "Front-End":
                    testList.Add(new ConnectionList { Name = "this server's IP, with the role of a " + serverRole, Hostname = machineIP, Ports = new List<int>() { 
                        ports[0], ports[1], ports[2], ports[3] 
                    } });
                    break;
                default: // Option for the "Deployment Controller with a Front-end" or if we cannot get the server role
                    testList.Add(new ConnectionList { Name = "this server's IP, with the role of a " + serverRole, Hostname = machineIP, Ports = new List<int>() { 
                        ports[0], ports[1], ports[2], ports[3], ports[4] 
                    } });
                    break;
            }

            // Set up connectivity tests for all registries in the OSSYS_SERVER table
            if (serverList != null)
            {
                foreach (var server in serverList)
                {
                    // Test only if it's not the server IP
                    if (server.Value == compilerServiceHostname && server.Value != machineIP)
                        testList.Add(new ConnectionList
                        {
                            Name = server.Key,
                            Hostname = server.Value,
                            Ports = new List<int>() {
                            ports[0], ports[1], ports[2], ports[3], ports[4]
                        }
                        }); // For Controller, include all ports

                    else if (server.Value != machineIP)
                        testList.Add(new ConnectionList
                        {
                            Name = server.Key,
                            Hostname = server.Value,
                            Ports = new List<int>() {
                            ports[0], ports[1], ports[2], ports[3]
                        }
                        }); // For Front-Ends, exclude the Deployment Controller port
                }
            }

            // Set up connectivity tests to cache invalidation service
            testList.Add(new ConnectionList { Name = "cache invalidation hostname set in the Configuration Tool", Hostname = cacheServiceHostname, Ports = new List<int>() { 
                ports[5] 
            } });

            // Set up connectivity tests to Controller server - ignore this test if we are inside the Controller server
            if (machineIP != compilerServiceHostname)
                testList.Add(new ConnectionList { Name = "Controller server hostname set in the Configuration Tool", Hostname = compilerServiceHostname, Ports = new List<int>() { 
                    ports[0], ports[1], ports[2], ports[3], ports[4] 
                } });

            // Set up connectivity test to MABS endpoint
            testList.Add(new ConnectionList { Name = "Mobile Apps Build Service", Hostname = "nativebuilder.api.outsystems.com/v1/GetHealth", Ports = new List<int>() { 
                ports[1] 
            } }); // Test only HTTPS

            // Set up connectivity test to the Service Studio auto updater
            testList.Add(new ConnectionList { Name = "Service Studio auto updater", Hostname = "api.outsystems.com/releaseshub/v1/latest?component=ServiceStudio&luv=11.0.0.200&version=0", Ports = new List<int>() { 
                ports[1] 
            } }); // Test only HTTPS

            // Set up connectivity test to Server.API
            testList.Add(new ConnectionList { Name = "Server.API", Hostname = "localhost/server.api/v1/health", Ports = new List<int>() { 
                ports[0] 
            } }); // Test only HTTP

            // Set up connectivity test to Server.Identity
            testList.Add(new ConnectionList { Name = "Server.Identity", Hostname = "localhost/server.identity/v1/health", Ports = new List<int>() { 
                ports[0] 
            } }); // Test only HTTP

            return testList;
        }
    }
}