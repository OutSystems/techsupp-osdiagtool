using System.Linq;
using System.Net.NetworkInformation;
using System.Net;

namespace OSDiagTool.Utils
{
    class NetworkUtils
    {
        /*
         *  Performs an ICMP echo request
         */
        public string PingAddress(string hostAddress)
        {
            Ping pinger = new Ping();
            IPAddress addressToPing = Dns.GetHostAddresses(hostAddress)
                .First(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            PingReply reply = pinger.Send(addressToPing);
            return reply.Address.ToString();
        }

        /*
         * Check if a TCP port is listening in the server
         */
        public bool IsPortListening (string port)
        {
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

                // Search for the port in the active TCP listeners list
                foreach (IPEndPoint endpoint in tcpConnInfoArray)
                {
                    if (endpoint.Port.ToString() == port)
                        return true;
                }
                return false;
            }
            // Exception thrown when an error occurs while retrieving network information.
            catch (NetworkInformationException e)
            {
                FileLogger.LogError("Failed to retrieve network information: ", e.Message + e.StackTrace);
                // Return false, since we could not validate the port
                return false;
            }
        }
    }
}
