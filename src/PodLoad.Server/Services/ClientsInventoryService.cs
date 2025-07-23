using PodLoad.Common.Contracts;
using PodLoad.Server.Models;

namespace PodLoad.Server.Services;

public class ClientsInventoryService(
    IConfiguration configuration,
    IHostApplicationLifetime applicationLifetime,
    ILogger<ClientsInventoryService> logger)
    : IClientsInventoryService
{
    private readonly ILogger<ClientsInventoryService> _logger = logger;
    private readonly CancellationToken _applicationStoppingToken = applicationLifetime.ApplicationStopping;
    private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("KeepAliveIntervalSeconds"));
    private Task? _updaterTask;
    private readonly Lock _updaterTaskLock = new();
    private readonly Lock _clientsLock = new();
    private readonly Dictionary<Guid, ClientInfo> _clients = new();

    public ClientReportResponse ProcessClientReport(ClientReportRequest request)
    {
        EnsurePeriodicUpdateEnabled();
        using var scope = _clientsLock.EnterScope();
        
        _logger.LogTrace("Received client report with ID: {0}", request.ClientId);
        
        if (_clients.TryGetValue(request.ClientId, out var clientInfo))
        {
            // Find an existing client, update TimeStamp and current params, then fire the event
            clientInfo.LastUpdated = DateTime.Now;
            clientInfo.Delay = request.Delay;
            clientInfo.MemoryAllocated = request.MemoryAllocated;
            clientInfo.State = ClientState.Active;
            
            _logger.LogTrace("Updating client with ID: {0}", clientInfo.Id);
            ClientUpdated?.Invoke(this, clientInfo);
        }
        else
        {
            // Create new client, fire event and return response from the created client
            clientInfo = new ClientInfo()
            {
                Id = request.ClientId,
                HostName = request.ClientHostName,
                IpAddress = request.ClientIpAddress,
                // when creating new client desired params are set to actual values
                Delay = request.Delay,
                DesiredDelay = request.Delay,
                MemoryAllocated = request.MemoryAllocated,
                DesiredMemoryAllocated = request.MemoryAllocated
            };
            _clients[clientInfo.Id] = clientInfo;
            
            _logger.LogInformation("Added new client with ID: {0}", clientInfo.Id);
            ClientAdded?.Invoke(this, clientInfo);
        }
        
        return new ClientReportResponse()
        {
            DesiredDelay = clientInfo.DesiredDelay,
            DesiredMemoryAllocated = clientInfo.DesiredMemoryAllocated,
            KeepAliveInterval = _keepAliveInterval
        };
    }

    public IEnumerable<ClientInfo> GetClients()
    {
        using var scope = _clientsLock.EnterScope();
        return _clients.Values
            .OrderBy(x => x.Id);
    }

    public event EventHandler<ClientInfo>? ClientAdded;
    public event EventHandler<ClientInfo>? ClientUpdated;
    public event EventHandler<ClientInfo>? ClientRemoved;

    private void EnsurePeriodicUpdateEnabled()
    {
        if (_updaterTask != null)
            return;

        lock (_updaterTaskLock)
        {
            if (_updaterTask != null)
                return;
            _updaterTask = CheckForClientsTimeout();
        }
    }

    private async Task CheckForClientsTimeout()
    {
        while (!_applicationStoppingToken.IsCancellationRequested)
        {
            var timeOutedClients = _clients.Values
                .Where(client => client.State == ClientState.Timeout || DateTime.Now - client.LastUpdated > _keepAliveInterval)
                .ToArray();

            if (timeOutedClients.Any())
            {
                using var scope = _clientsLock.EnterScope();

                foreach (var client in timeOutedClients)
                {
                    if (client.State == ClientState.Timeout)
                    {
                        _clients.Remove(client.Id);
                        _logger.LogInformation("Removed client with ID: {0}", client.Id);
                        ClientRemoved?.Invoke(this, client);
                    }
                    else
                    {
                        client.State = ClientState.Timeout;
                        _logger.LogInformation("First timeout for client with ID: {0}", client.Id);
                        ClientUpdated?.Invoke(this, client);
                    }
                }
            }
            
            await Task.Delay(_keepAliveInterval, _applicationStoppingToken);
        }
    }
}