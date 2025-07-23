using System;
using System.Linq;
using System.Net;

namespace PodLoad.Common.Services
{
    public class HostInfoService : IHostInfoService
    {
        public string GetHostName()
        {
            return Environment.MachineName;
        }

        public string GetHostIpv4Address()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .First(ips => ips is { IsIPv6LinkLocal: false, IsIPv6Multicast: false, IsIPv6SiteLocal: false })
                .ToString();
        }
    }
}