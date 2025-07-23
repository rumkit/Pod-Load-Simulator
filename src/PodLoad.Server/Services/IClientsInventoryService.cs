using PodLoad.Common.Contracts;
using PodLoad.Server.Models;

namespace PodLoad.Server.Services;

public interface IClientsInventoryService
{
    ClientReportResponse ProcessClientReport(ClientReportRequest request);
    IEnumerable<ClientInfo> GetClients();

    event EventHandler<ClientInfo> ClientAdded;
    event EventHandler<ClientInfo> ClientUpdated;
    event EventHandler<ClientInfo> ClientRemoved;
}