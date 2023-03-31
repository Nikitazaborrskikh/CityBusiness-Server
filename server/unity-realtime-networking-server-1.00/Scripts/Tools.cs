﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DevelopersHub.RealtimeNetworking.Server
{
    class Tools
    {

        public static string GenerateToken()
        {
            return Path.GetRandomFileName().Remove(8, 1);
        }

        public static string GetIP(AddressFamily type)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == type)
                {
                    return ip.ToString();
                }
            }
            return "0.0.0.0";
        }

        public static string GetExternalIP()
        {
            try
            {
                var ip = IPAddress.Parse(new WebClient().DownloadString("https://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim());
                return ip.ToString();
            }
            catch (Exception)
            {
                try
                {
                    StreamReader sr = new StreamReader(WebRequest.Create("https://checkip.dyndns.org").GetResponse().GetResponseStream());
                    string[] ipAddress = sr.ReadToEnd().Trim().Split(':')[1].Substring(1).Split('<');
                    return ipAddress[0];
                }
                catch (Exception)
                {
                    return "0.0.0.0";
                }
            }
        }

    }
}