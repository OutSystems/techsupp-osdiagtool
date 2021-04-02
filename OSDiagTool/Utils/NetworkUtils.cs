using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

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
         * Check if a TCP port is open in the server
         */
        public bool IsPortOpen (string address, int port)
        {
            TcpClient tc = null;
            try
            {
                tc = new TcpClient(address, port);
                // If we get here, port is open
                return true;
            } 
            catch (SocketException se) 
            {
                // If we get here, port is not open, or host is not reachable
                FileLogger.LogError("Failed to check if port is open: ", se.Message + se.StackTrace);
                return false;
            }
            finally
            {
                // Close the tcp conection
                if (tc != null)
                    tc.Close();
            }
        }
    }
}
