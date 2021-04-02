using OSDiagTool.Utils;
using System;
using System.IO;

namespace OSDiagTool.Platform.Requirements
{
    class PlatformRequirements
    {
        public static void ValidateRequirements(string dbEngine, string reqFilePath, OSDiagToolConf.ConfModel.strConfModel configurations, OSDiagToolForm.OsDiagFormConfModel.strFormConfigurationsModel FormConfigurations,
            DBConnector.SQLConnStringModel sqlConnString = null, DBConnector.OracleConnStringModel oracleConnString = null)
        {
            bool checkNetworkRequirements = false;

            // Getting the ports set in Configuration Tool (server.hsconf file)
            int[] portArray = {
                80,
                443,
                Int32.Parse(Platform.PlatformUtils.GetServiceConfigurationValue("DeploymentServerPort")) // Default port 12001
            };

            // Write the results to log file
            using (TextWriter writer = new StreamWriter(File.Create(Path.Combine(reqFilePath, "requirements.log"))))
            {
                writer.WriteLine(string.Format("Platfrom Requeriments\n\n{0}: [INFO] Validating Network Requirements\n", DateTime.Now.ToString()));

                NetworkUtils check = new NetworkUtils();

                /* TODO: validate if we are:
                    *  Inside a Controller
                    *  Inside a pure Controller
                    *  Inside a front-end from a farm 
                    */

                // Get server IP address
                writer.WriteLine(string.Format("{0}: [INFO] Retrieving IP address from this server...", DateTime.Now.ToString()));
                string serverIP = check.PingAddress("");
                writer.WriteLine(string.Format("{0}: [INFO] IP address detected in this server: {1}.\n", DateTime.Now.ToString(), serverIP));

                // Validate if localhost is resolving to 127.0.0.1
                writer.WriteLine(string.Format("{0}: [INFO] Perform a ping request to localhost...", DateTime.Now.ToString()));
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
