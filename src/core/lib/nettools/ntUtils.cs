using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace nettools
{
    public struct NtHostInfo
    {
        public string Hostname;
        public uint HostAddr;
        public uint Result;
    }

    public static class NtUtils
    {
        public static bool GetHostInfo(string hostname, ref NtHostInfo hostInfo)
        {
            hostInfo = new NtHostInfo
            {
                Hostname = hostname,
                HostAddr = 0,
                Result = 0
            };

            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
                if (hostEntry.AddressList.Length > 0)
                {
                    hostInfo.HostAddr = (uint)hostEntry.AddressList[0].Address;
                    return true;
                }
            }
            catch (SocketException ex)
            {
                hostInfo.Result = (uint)ex.ErrorCode;
            }

            return false;
        }

        public static bool IsConnectedToInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public static string LongToIPv4(uint rawIp)
        {
            IPAddress ipAddress = new IPAddress(rawIp);
            return ipAddress.ToString();
        }
    }
}
