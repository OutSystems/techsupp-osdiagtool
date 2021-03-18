using System.Linq;
using System.Net.NetworkInformation;
using System.Net;

namespace OSDiagTool.Utils
{
    class NetworkUtils
    {
        public string PingAddress(string hostAddress)
        {
            // ICMP echo request
            Ping pinger = new Ping();
            IPAddress addressToPing = Dns.GetHostAddresses(hostAddress)
                .First(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            PingReply reply = pinger.Send(addressToPing);
            return reply.Address.ToString();
        }

        public bool IsPortListening (int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            // Search for the port in the active TCP listeners list
            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Port == port)
                    return true;
            }
            return false;
        }
    }
}
