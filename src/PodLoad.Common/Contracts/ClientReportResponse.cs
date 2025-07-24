using System;

namespace PodLoad.Common.Contracts
{
    public class ClientReportResponse
    {
        public int DesiredPercentage { get; set; }
        public uint DesiredMemoryAllocated { get; set; }
        public TimeSpan KeepAliveInterval { get; set; } 
    }
}