using PodLoad.Client;
using PodLoad.Common.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IHostInfoService, HostInfoService>();

var host = builder.Build();
host.Run();
