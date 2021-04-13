using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

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
         * Opens a TCP stream to an address and a port
         */
        public string OpenTcpStream (string address, int port)
        {
            TcpClient tcpClient = null;
            try
            {  
                tcpClient = new TcpClient(address, port);
                // Let's try sending the bare minimum to compose a request
                var request = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: " + address + ":" + port + "\r\nConnection: Close\r\n\r\n");

                NetworkStream stream = tcpClient.GetStream();
                stream.ReadTimeout = 1000;
                stream.Write(request, 0, request.Length);
                stream.Flush();

                int bytesRead = stream.Read(request, 0, request.Length);
                // Returning the response string
                return Encoding.ASCII.GetString(request, 0, bytesRead);
            }
            catch (Exception e) 
            {
                if (IsPortInUse(port))
                    // Let's check if the port is in use
                    return "The port " + port + " is in use.";
                else
                {
                    // If we get here, something else happened, like the port is not open, or host is not reachable
                    FileLogger.LogError("Network stream failed with the error: ", e.Message + e.StackTrace);
                    return null;
                }
            }
            finally
            {
                // Close the tcp conection and the underlying network stream
                if (tcpClient != null)
                    tcpClient.Close();
            }
        }

        /*
         * Checks if a port is an active TCP listener (all TCP states except the Listen state)
         */
        public static bool IsPortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
    }
}
