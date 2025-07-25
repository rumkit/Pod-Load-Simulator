using System.Net.Http.Json;
using System.Text.Json;
using PodLoad.Common.Contracts;
using PodLoad.Common.Services;

namespace PodLoad.Client;

public class Worker(ILogger<Worker> logger, IConfiguration configuration, IHostInfoService hostInfoService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Load defaults
        var clientSettings = configuration.GetSection("ClientSettings");
        var percentage = clientSettings.GetValue<int>("DefaultPercentage");
        var memoryToAllocate = clientSettings.GetValue<uint>("DefaultMemory_Mb");
        var clientId = Guid.CreateVersion7();
        var serverAddress = clientSettings.GetValue<string>("ServerAddress");
        var serverPort = clientSettings.GetValue<int>("ServerPort");
        var reportEndpoint =  clientSettings.GetValue<string>("ReportEndpoint");
        var hostName = hostInfoService.GetHostName();
        var hostIpAddress = hostInfoService.GetHostIpv4Address();

        // Start load simulation cycle and allocate memory chunk
        var cpuLoadSimulator = new CpuLoadSimulator(percentage, stoppingToken);
        using var memoryKeeper = new MemoryKeeper();
        memoryKeeper.Allocate(memoryToAllocate * 1024 * 1024);
        
        
        logger.LogInformation("Started service on host: {0} with IP: {1}. Client ID: {2}", hostName, hostIpAddress, clientId);
        logger.LogInformation("Using default params - Memory size: {0} MB, CPU Load: {1}%", memoryToAllocate, percentage);
        
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
                    Percentage = percentage,
                    MemoryAllocated = memoryToAllocate
                });
                
                // Send report request to server and parse response
                var response = await client.PostAsync(reportUri, content, stoppingToken);
                response.EnsureSuccessStatusCode();
                var clientReportResponse = JsonSerializer.Deserialize<ClientReportResponse>(await response.Content.ReadAsStringAsync(stoppingToken), JsonSerializerOptions.Web);
                if (clientReportResponse == null)
                    throw new ApplicationException("No response body from server's reply");
                
                // Adjust percentage and memory
                if (percentage != clientReportResponse.DesiredPercentage)
                {
                    percentage = clientReportResponse.DesiredPercentage;
                    cpuLoadSimulator.Percentage = percentage;
                    logger.LogInformation("New CPU load: {0}%", percentage);
                }

                if (memoryToAllocate != clientReportResponse.DesiredMemoryAllocated)
                {
                    memoryToAllocate = clientReportResponse.DesiredMemoryAllocated;
                    memoryKeeper.Allocate(memoryToAllocate * 1024 * 1024);
                    logger.LogInformation("New memory size: {0} MB", memoryToAllocate);;
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
}