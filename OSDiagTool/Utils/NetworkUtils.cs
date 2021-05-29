using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System;
using System.Net.Security;

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
         * Opens a HTTP or HTTPS web request to an address and a port
         * Returns the status code
         */
        public static string OpenWebRequest(string address, int port)
        {
            HttpWebRequest request;
            if (port == 443)
                request = (HttpWebRequest)WebRequest.Create("https://" + address);
            else
                request = (HttpWebRequest)WebRequest.Create("http://" + address);

            request.Method = "GET";

            try
            {
                // Ignore the certificate trust validation - we just need to know the response
                ServicePointManager.ServerCertificateValidationCallback = new
                RemoteCertificateValidationCallback
                (
                   delegate { return true; }
                );

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    string result = ((int)response.StatusCode).ToString();
                    response.Close();

                    return result;
                }
            }
            // For status codes >= 400
            catch (WebException we)
            {
                return ((int)((HttpWebResponse)we.Response).StatusCode).ToString();
            }
            catch (Exception e)
            {
                FileLogger.LogError("Web request failed with the error: ", e.Message + e.StackTrace);
                return null;
            }
        }

        /*
         * Opens a TCP request request to an address and a port
         * Returns if we were able to connect to the port or if it's in use 
         */
        public static string ConnectToPort(string address, int port, bool retrieveIP = false)
        {
            TcpClient tcpClient = null;

            try
            {
                tcpClient = new TcpClient(address, port);
                // If we reached here, then we successfully connected to the port

                // If requested, retrieve the remote IP
                if (retrieveIP)
                {
                    IPEndPoint remoteIpEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                    return remoteIpEndPoint.Address.ToString();
                }

                return "Connected";
            }
            catch (Exception e)
            {
                // Since we could not connect, let's check if the port is in use
                if (IsPortInUse(port))
                    return "The port " + port + " is opened but its currently in use";
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
                {
                    tcpClient.Client.Close();
                    tcpClient.Close();
                }
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

        // Check if a hostname is an IP or not
        public static bool IsIP(string hostname)
        {
            return (Uri.CheckHostName(hostname) == UriHostNameType.IPv4 || Uri.CheckHostName(hostname) == UriHostNameType.IPv6);
        }
    }
}
