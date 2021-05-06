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
        public static string PingAddress(string hostAddress)
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
        public static string OpenTcpStream (string address, int port)
        {
            TcpClient tcpClient = null;

            try
            {  
                tcpClient = new TcpClient(address, port);
                // If we reached here, then we could connect to the port

                // We are not waiting for a status code response for ports like 12001 or 5672, for example
                if (port != 80 && port != 443) 
                    return "Port listening";

                // Getting a status code
                // Setting the HTTP protocol
                string httpProtocol = "HTTP/1.1 ";

                // Let's try sending the bare minimum to compose a request
                var request = Encoding.ASCII.GetBytes("GET / " + httpProtocol + "\r\nHost: " + address + ":" + port + "\r\nConnection: Close\r\n\r\n");

                NetworkStream stream = tcpClient.GetStream();
                // Wait 1 second for the response
                stream.ReadTimeout = 1000;
                stream.Write(request, 0, request.Length);
                stream.Flush();

                int bytesRead = stream.Read(request, 0, request.Length);
                // Returning the response string and getting the status code
                string response = Encoding.ASCII.GetString(request, 0, bytesRead);

                // The status code is after the HTTP protocol
                string statusCode = response.Substring(response.IndexOf(httpProtocol) + httpProtocol.Length, 3);

                // Validating the status code
                if (int.TryParse(statusCode, out _))
                    return "Status code " + statusCode;
                else
                    return "Could not parse status code - returned " + statusCode;
            }
            catch (Exception e) 
            {
                // Let's check if the port is in use
                if (IsPortInUse(port))
                    return "Port " + port + " is opened but its currently in use";
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
        private static bool IsPortInUse(int port)
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

        /*
         * Checks if any network interface is marked as "up" and is not a loopback or tunnel interface.
         * Keep in mind that this does not check for internet connections
         */
        public static bool IsNetworkUp()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }
    }
}
