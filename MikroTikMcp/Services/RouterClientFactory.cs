using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MikroTikMcp.Services;

public sealed class RouterClientFactory
{
    private readonly HttpClientHandler _handler;
    private readonly MikroTikSettings _centralSettings;
    private readonly RemoteRouterSettings _remoteSettings;
    private readonly ILogger<MikroTikClient> _clientLogger;
    private readonly ConcurrentDictionary<string, MikroTikClient> _clients = new(StringComparer.OrdinalIgnoreCase);
    private volatile Dictionary<string, string>? _nameToIp;

    public MikroTikClient Central { get; }

    public RouterClientFactory(
        MikroTikSettings centralSettings,
        RemoteRouterSettings remoteSettings,
        ILogger<MikroTikClient> clientLogger)
    {
        _centralSettings = centralSettings;
        _remoteSettings = remoteSettings;
        _clientLogger = clientLogger;
        _handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        Central = CreateClient(
            centralSettings.Host, centralSettings.Port,
            centralSettings.Protocol, centralSettings.User, centralSettings.Password);
    }

    public async Task<MikroTikClient> GetClientAsync(string? router, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(router) || router.Equals("central", StringComparison.OrdinalIgnoreCase))
            return Central;

        var ip = IPAddress.TryParse(router, out _)
            ? router
            : await ResolveNameAsync(router, ct);

        return _clients.GetOrAdd(ip, addr => CreateClient(
            addr, _remoteSettings.Port, _remoteSettings.Protocol,
            _remoteSettings.User, _remoteSettings.Password));
    }

    public void InvalidateCache() => _nameToIp = null;

    private MikroTikClient CreateClient(string host, int port, string protocol, string user, string password)
    {
        var http = new HttpClient(_handler, disposeHandler: false);
        var settings = new MikroTikSettings
        {
            Host = host, Port = port, Protocol = protocol,
            User = user, Password = password
        };
        return new MikroTikClient(http, settings, _clientLogger);
    }

    private async Task<string> ResolveNameAsync(string name, CancellationToken ct)
    {
        var map = _nameToIp;
        if (map is null)
        {
            map = await BuildNameMapAsync(ct);
            _nameToIp = map;
        }

        return map.TryGetValue(name, out var ip)
            ? ip
            : throw new ArgumentException(
                $"Router '{name}' not found. Use list_routers to see available routers.");
    }

    private async Task<Dictionary<string, string>> BuildNameMapAsync(CancellationToken ct)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // PPP active connections first (these are definitely online)
        try
        {
            var active = await Central.GetAsync("/ppp/active", ct);
            if (active.ValueKind == JsonValueKind.Array)
            {
                foreach (var c in active.EnumerateArray())
                {
                    var n = c.GetProperty("name").GetString() ?? "";
                    var addr = c.GetProperty("address").GetString() ?? "";
                    if (n.Length > 0 && addr.Length > 0) map.TryAdd(n, addr);
                }
            }
        }
        catch { /* No active PPP */ }

        // WireGuard peers second — only connected (not disabled, has endpoint)
        try
        {
            var peers = await Central.GetAsync("/interface/wireguard/peers", ct);
            if (peers.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in peers.EnumerateArray())
                {
                    var disabled = p.GetProperty("disabled").GetString() == "true";
                    if (disabled) continue;

                    var endpoint = p.TryGetProperty("current-endpoint-address", out var ep) ? ep.GetString() : "";
                    if (string.IsNullOrEmpty(endpoint)) continue;

                    var n = p.GetProperty("name").GetString() ?? "";
                    var aa = p.GetProperty("allowed-address").GetString() ?? "";
                    var ip = aa.Split(',').FirstOrDefault()?.Split('/').FirstOrDefault() ?? "";
                    if (n.Length > 0 && ip.Length > 0) map.TryAdd(n, ip);
                }
            }
        }
        catch { /* WireGuard not configured */ }

        return map;
    }
}
