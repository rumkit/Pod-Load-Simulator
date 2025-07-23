using System;

namespace PodLoad.Common.Contracts
{
    public class ClientReportResponse
    {
        public TimeSpan DesiredDelay { get; set; }
        public ulong DesiredMemoryAllocated { get; set; }
        public TimeSpan KeepAliveInterval { get; set; } 
    }
}