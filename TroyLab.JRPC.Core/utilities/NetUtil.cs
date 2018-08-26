using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TroyLab.JRPC.Core
{
    public class NetUtil
    {
        public static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public static int GetFreePortInRange(int PortStartIndex, int PortEndIndex)
        {
            try
            {
                Random r = new Random(Guid.NewGuid().GetHashCode());
                int rInt = r.Next(PortStartIndex, PortEndIndex - 100); //for ints
                PortStartIndex = rInt;

                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

                IPEndPoint[] tcpEndPoints = ipGlobalProperties.GetActiveTcpListeners();
                List<int> usedServerTCpPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();

                IPEndPoint[] udpEndPoints = ipGlobalProperties.GetActiveUdpListeners();
                List<int> usedServerUdpPorts = udpEndPoints.Select(p => p.Port).ToList<int>();

                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
                List<int> usedPorts = tcpConnInfoArray.Where(p => p.State != TcpState.Closed).Select(p => p.LocalEndPoint.Port).ToList<int>();

                usedPorts.AddRange(usedServerTCpPorts.ToArray());
                usedPorts.AddRange(usedServerUdpPorts.ToArray());

                int unusedPort = 0;

                for (int port = PortStartIndex; port < PortEndIndex; port++)
                {
                    if (!usedPorts.Contains(port))
                    {
                        unusedPort = port;
                        break;
                    }
                }

                if (unusedPort == 0)
                {
                    throw new ApplicationException("GetFreePortInRange, Out of ports");
                }

                return unusedPort;
            }
            catch (Exception ex)
            {

                string errorMessage = ex.Message;
                throw;
            }
        }
    }
}
