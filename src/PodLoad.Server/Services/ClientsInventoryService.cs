using PodLoad.Common.Contracts;

namespace PodLoad.Server.Services;

public interface IClientsInventoryService
{
    ClientReportResponse ProcessClientReport(ClientReportRequest request);
}

public class ClientsInventoryService : IClientsInventoryService
{
    public ClientReportResponse ProcessClientReport(ClientReportRequest request)
    {
        return new ClientReportResponse();
    }
}