using System;
using System.Net;

namespace PodLoad.Common.Contracts
{
    public class ClientReportRequest
    {
        public Guid ClientId { get; set; }
        public string ClientHostName { get; set; }
        public string ClientIpAddress { get; set; }
        public TimeSpan Delay { get; set; }
        public uint MemoryAllocated { get; set; }
    }
}