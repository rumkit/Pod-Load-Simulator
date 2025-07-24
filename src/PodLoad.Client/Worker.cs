using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using PodLoad.Common.Contracts;
using PodLoad.Common.Services;

namespace PodLoad.Client;

public class Worker(ILogger<Worker> logger, IConfiguration configuration, IHostInfoService hostInfoService) : BackgroundService
{
    private TimeSpan _loadDelay;
    private uint _memoryToAllocate;
    private IntPtr _memoryChunk;
    private Task _loadTask;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Load defaults
        var clientSettings = configuration.GetSection("ClientSettings");
        _loadDelay = TimeSpan.FromMilliseconds(clientSettings.GetValue<int>("DefaultDelay_ms"));
        _memoryToAllocate = clientSettings.GetValue<uint>("DefaultMemory_Mb");
        var clientId = Guid.CreateVersion7();
        var serverAddress = clientSettings.GetValue<string>("ServerAddress");
        var serverPort = clientSettings.GetValue<int>("ServerPort");
        var reportEndpoint =  clientSettings.GetValue<string>("ReportEndpoint");
        var hostName = hostInfoService.GetHostName();
        var hostIpAddress = hostInfoService.GetHostIpv4Address();

        // Start load simulation cycle and allocate memory chunk
        _loadTask = Task.Run(() => SimulateLoadAsync(stoppingToken), stoppingToken);
        ReallocateMemory();
        
        
        logger.LogInformation("Started service on host: {0} with IP: {1}. Client ID: {2}", hostName, hostIpAddress, clientId);
        logger.LogInformation("Using default params - Memory size: {0} MB, Delay: {1}", _memoryToAllocate, _loadDelay);
        
        // Build report URI and prepare http client
        var uriBuilder = new UriBuilder() { Scheme = "http", Host = serverAddress, Port = serverPort, Path = reportEndpoint };
        using var client = new HttpClient();
        var reportUri = uriBuilder.Uri;

        logger.LogInformation("Reports endpoint set to {0}", reportUri);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan requestTimeOut;
            try
            {
                using var content = JsonContent.Create(new ClientReportRequest()
                {
                    ClientId = clientId,
                    ClientHostName = hostName,
                    ClientIpAddress = hostIpAddress.ToString(),
                    Delay = _loadDelay,
                    MemoryAllocated = _memoryToAllocate
                });
                
                // Send report request to server and parse response
                var response = await client.PostAsync(reportUri, content, stoppingToken);
                response.EnsureSuccessStatusCode();
                var clientReportResponse = JsonSerializer.Deserialize<ClientReportResponse>(await response.Content.ReadAsStringAsync(stoppingToken), JsonSerializerOptions.Web);
                if (clientReportResponse == null)
                    throw new ApplicationException("No response body from server's reply");
                
                // Adjust delay
                _loadDelay = clientReportResponse.DesiredDelay;

                // If desired memory size has changed - reallocate the memory chunk
                if (clientReportResponse.DesiredMemoryAllocated != _memoryToAllocate)
                {
                    _memoryToAllocate = clientReportResponse.DesiredMemoryAllocated;
                    ReallocateMemory();
                    logger.LogInformation("New memory size: {0} MB" , _memoryToAllocate);
                }
                
                // Set request timeout to a half of the keep-alive interval
                requestTimeOut = clientReportResponse.KeepAliveInterval / 2;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error during sending report");
                requestTimeOut = TimeSpan.FromSeconds(15);
            }
            
            await Task.Delay(requestTimeOut, stoppingToken);
        }
    }

    private void ReallocateMemory()
    {
        if(_memoryChunk != IntPtr.Zero)
            Marshal.FreeHGlobal(_memoryChunk);

        var sizeInBytes = (int)_memoryToAllocate * 1024 * 1024;
        _memoryChunk = Marshal.AllocHGlobal(sizeInBytes);
    }

    private async Task SimulateLoadAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_loadDelay, stoppingToken);
        }
    }
}
