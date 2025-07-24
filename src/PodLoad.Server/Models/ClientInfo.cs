using System.Net;

namespace PodLoad.Server.Models;

public class ClientInfo
{
    public Guid Id { get; set; }
    public string HostName { get; set; }
    public string IpAddress { get; set; }
    public uint MemoryAllocated { get; set; }
    public uint DesiredMemoryAllocated { get; set; }
    public TimeSpan Delay { get; set; }
    public TimeSpan DesiredDelay { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public ClientState State { get; set; } = ClientState.Active;
}

public enum ClientState
{
    Active,
    Timeout
}