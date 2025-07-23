using MudBlazor.Services;
using PodLoad.Common.Contracts;
using PodLoad.Common.Services;
using PodLoad.Server.Components;
using PodLoad.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IHostInfoService, HostInfoService>();
builder.Services.AddSingleton<IClientsInventoryService, ClientsInventoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost(
    "/api/client-report", 
    (IClientsInventoryService clientsInventoryService, ClientReportRequest request) => clientsInventoryService.ProcessClientReport(request)
    );

app.Run();