using System;
using System.Data.SqlClient;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using OSDiagTool.Utils;

namespace OSDiagTool.Platform.Requirements
{
    class PlatformRequirements
    {
        public static void ValidateRequirements(string dbEngine, string reqFilePath, OSDiagToolConf.ConfModel.strConfModel configurations, DBConnector.SQLConnStringModel sqlConnString = null, 
            DBConnector.OracleConnStringModel oracleConnString = null)
        {
            bool checkNetworkRequirements = false;
            string compilerServiceHostName = null;

            // Getting the ports set in Configuration Tool (server.hsconf file)
            int[] portArray = {
                80,
                443,
                Int32.Parse(Platform.PlatformUtils.GetServiceConfigurationValue("DeploymentServerPort")) // Default port 12001
            };

            // Getting the information that we need from the database
            if (dbEngine.Equals("sqlserver"))
            {
                var connector = new DBConnector.SLQDBConnector();
                SqlConnection connection = connector.SQLOpenConnection(sqlConnString);

                using (connection)
                {
                    // Retrieving the CompilerService.Hostname value from the parameters table
                    compilerServiceHostName = Platform.PlatformUtils.GetCompilerServiceHostName(dbEngine, configurations.queryTimeout, connection);
                }
                connector.SQLCloseConnection(connection);
            }
            else if (dbEngine.Equals("oracle"))
            {
                var connector = new DBConnector.OracleDBConnector();
                OracleConnection connection = connector.OracleOpenConnection(oracleConnString);

                string platformDBAdminUser = Platform.PlatformUtils.GetPlatformDBAdminUser();

                // Same operations as SQL Server
                using (connection)
                {
                    compilerServiceHostName = Platform.PlatformUtils.GetCompilerServiceHostName(dbEngine, configurations.queryTimeout, null, connection, platformDBAdminUser);
                }
                connector.OracleCloseConnection(connection);
            }

            // Something is wrong if we cannot retrieve the compiler service hostname from the database
            if (compilerServiceHostName == null)
            {
                throw new Exception("Failed to retrieve the compiler service hostname from the database");
            }

            // Write the results to log file
            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(reqFilePath, "requirements.log"))))
            {
                writer.WriteLine(string.Format("Platfrom Requeriments\n\n{0}: [INFO] Validating Network Requirements\n", DateTime.Now.ToString()));

                NetworkUtils check = new NetworkUtils();

                /* TODO: validate if we are:
                    *  Inside a pure Controller
                    */

                // Get server IP address
                writer.WriteLine(string.Format("{0}: [INFO] Retrieving IP address from this server...", DateTime.Now.ToString()));
                string serverIP = check.PingAddress("");
                writer.WriteLine(string.Format("{0}: [INFO] IP address detected in this server: {1}.", DateTime.Now.ToString(), serverIP));

                // Check if we are inside a controller server
                if (serverIP == compilerServiceHostName)
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that we are inside a server with a Deployment Controller role - IP address is the same as the compiler service hostname.\n", DateTime.Now.ToString()));
                else
                    writer.WriteLine(string.Format("{0}: [INFO] Detected that we are inside a server with a Front-end role.\n", DateTime.Now.ToString()));

                // Validate if localhost is resolving to 127.0.0.1
                writer.WriteLine(string.Format("{0}: [INFO] Performing a ping request to localhost...", DateTime.Now.ToString()));
                string reply = check.PingAddress("localhost");
                if (reply == "127.0.0.1")
                    writer.WriteLine(string.Format("{0}: [INFO] Localhost resolves to {1}.\n", DateTime.Now.ToString(), reply));
                else
                {
                    writer.WriteLine(string.Format("{0}: [WARNING] Localhost is resolving to {1} instead of 127.0.0.1.\n", DateTime.Now.ToString(), reply));
                    checkNetworkRequirements = true;
                }

                // Validate ports
                writer.WriteLine(string.Format("{0}: [INFO] Checking if the required ports are open for the IP {1}...", DateTime.Now.ToString(), serverIP));
                foreach (int port in portArray)
                {
                    // Check ports for the server IP
                    if (check.IsPortOpen(serverIP, port))
                    {
                        writer.WriteLine(string.Format("{0}: [INFO] The TCP port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, serverIP));
                    }
                    else
                    {
                        writer.WriteLine(string.Format("{0}: [WARNING] Could not detect if port {1} is open for the IP {2}.", DateTime.Now.ToString(), port, serverIP));
                        checkNetworkRequirements = true;
                    }
                }

                if (checkNetworkRequirements)
                    writer.WriteLine("\n[WARNING] Please review the OutSystems Network Requirements.");
            }

        }
    }
}
