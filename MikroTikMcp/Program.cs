using Elastic.CommonSchema.Serilog;
using MikroTikMcp.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: structured JSON logs in ECS format for Filebeat → Elasticsearch
builder.Host.UseSerilog((context, config) =>
{
    var logPath = context.Configuration["Logging:FilePath"] ?? "logs/network-tools-.json";
    var rollingInterval = Enum.TryParse<RollingInterval>(
        context.Configuration["Logging:RollingInterval"], true, out var ri) ? ri : RollingInterval.Day;

    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("service.name", "network-tools")
        .Enrich.WithProperty("service.version", "3.0.0")
        .Enrich.WithProperty("service.environment",
            context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console()
        .WriteTo.File(
            formatter: new EcsTextFormatter(),
            path: logPath,
            rollingInterval: rollingInterval,
            retainedFileCountLimit: 30,
            shared: true);
});

// MikroTik connection settings — all required, no silent fallbacks
var mikrotikSection = builder.Configuration.GetSection("MikroTik");
if (!mikrotikSection.Exists())
    throw new InvalidOperationException("Missing 'MikroTik' section in appsettings. Cannot start without router configuration.");

var mikrotikSettings = new MikroTikSettings
{
    Host = mikrotikSection["Host"] ?? throw new InvalidOperationException("MikroTik:Host is required"),
    Port = int.TryParse(mikrotikSection["Port"], out var p) ? p : throw new InvalidOperationException("MikroTik:Port is required"),
    Protocol = mikrotikSection["Protocol"] ?? throw new InvalidOperationException("MikroTik:Protocol is required"),
    User = mikrotikSection["User"] ?? throw new InvalidOperationException("MikroTik:User is required"),
    Password = mikrotikSection["Password"] ?? throw new InvalidOperationException("MikroTik:Password is required"),
};

builder.Services.AddSingleton(mikrotikSettings);

var remoteSection = builder.Configuration.GetSection("RemoteRouters");
if (!remoteSection.Exists())
    throw new InvalidOperationException("Missing 'RemoteRouters' section in appsettings. Cannot start without remote router configuration.");

var remoteRouterSettings = new RemoteRouterSettings
{
    User = remoteSection["User"] ?? throw new InvalidOperationException("RemoteRouters:User is required"),
    Password = remoteSection["Password"] ?? throw new InvalidOperationException("RemoteRouters:Password is required"),
    Port = int.TryParse(remoteSection["Port"], out var rp) ? rp : throw new InvalidOperationException("RemoteRouters:Port is required"),
    Protocol = remoteSection["Protocol"] ?? throw new InvalidOperationException("RemoteRouters:Protocol is required"),
};
builder.Services.AddSingleton(remoteRouterSettings);

// Router client factory manages connections to all routers (central + remotes)
builder.Services.AddSingleton<RouterClientFactory>();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "network-tools",
            Version = "3.0.0",
        };
    })
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("client.ip", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        diagnosticContext.Set("user_agent.original", httpContext.Request.Headers.UserAgent.ToString());
    };
});

app.MapMcp().AllowAnonymous();

app.Run();
