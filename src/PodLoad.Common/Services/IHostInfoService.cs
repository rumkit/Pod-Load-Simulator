namespace PodLoad.Common.Services
{
    public interface IHostInfoService
    {
        string GetHostName();
        string GetHostIpv4Address();
    }
}