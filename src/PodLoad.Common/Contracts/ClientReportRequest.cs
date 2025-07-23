using System;
using System.Net;

namespace PodLoad.Common.Contracts
{
    public class ClientReportRequest
    {
        public Guid ClientId { get; set; }
        public string ClientHostName { get; set; }
        public IPAddress ClientIpAddress { get; set; }
        public TimeSpan Delay { get; set; }
        public ulong MemoryAllocated { get; set; }
    }
}