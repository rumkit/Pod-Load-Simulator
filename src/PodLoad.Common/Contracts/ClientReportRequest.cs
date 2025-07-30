using System;

namespace PodLoad.Common.Contracts
{
    public class ClientReportRequest
    {
        public Guid ClientId { get; set; }
        public string ClientHostName { get; set; }
        public string ClientIpAddress { get; set; }
        public int Percentage { get; set; }
        public uint MemoryAllocated { get; set; }
    }
}