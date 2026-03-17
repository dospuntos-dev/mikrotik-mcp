using System.ComponentModel;
using System.Text.Json;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class RouterTools
{
    [McpServerTool(Name = "list_routers", ReadOnly = true)]
    [Description("List all routers: central and VPN-connected remotes (WireGuard/SSTP). Returns names and tunnel IPs that can be used as the 'router' parameter in other tools.")]
    public static async Task<string> ListRouters(
        RouterClientFactory factory,
        CancellationToken ct)
    {
        var client = factory.Central;
        factory.InvalidateCache();
        var routers = new List<object>();

        // Central router
        routers.Add(new { name = "central", type = "central" });

        // WireGuard peers
        try
        {
            var wgPeers = await client.GetAsync("/interface/wireguard/peers", ct);
            if (wgPeers.ValueKind == JsonValueKind.Array)
            {
                foreach (var peer in wgPeers.EnumerateArray())
                {
                    var disabled = peer.GetProperty("disabled").GetString() == "true";
                    var name = peer.GetProperty("name").GetString() ?? "";
                    var allowedAddress = peer.GetProperty("allowed-address").GetString() ?? "";
                    var lastHandshake = peer.TryGetProperty("last-handshake", out var hs) ? hs.GetString() : "never";
                    var currentEndpoint = peer.TryGetProperty("current-endpoint-address", out var ep) ? ep.GetString() : "";
                    var tunnelIp = allowedAddress.Split(',').FirstOrDefault()?.Split('/').FirstOrDefault() ?? "";

                    routers.Add(new
                    {
                        name,
                        type = "wireguard",
                        tunnel_ip = tunnelIp,
                        allowed_address = allowedAddress,
                        status = disabled ? "disabled" : (string.IsNullOrEmpty(currentEndpoint) ? "no-handshake" : "connected"),
                        last_handshake = lastHandshake
                    });
                }
            }
        }
        catch (MikroTikException) { }

        // PPP active connections (SSTP, L2TP, etc.)
        try
        {
            var pppActive = await client.GetAsync("/ppp/active", ct);
            if (pppActive.ValueKind == JsonValueKind.Array)
            {
                foreach (var conn in pppActive.EnumerateArray())
                {
                    var name = conn.GetProperty("name").GetString() ?? "";
                    var address = conn.GetProperty("address").GetString() ?? "";
                    var service = conn.GetProperty("service").GetString() ?? "";
                    var uptime = conn.GetProperty("uptime").GetString() ?? "";

                    routers.Add(new
                    {
                        name,
                        type = service,
                        tunnel_ip = address,
                        status = "connected",
                        uptime
                    });
                }
            }
        }
        catch (MikroTikException) { }

        return Json.Format(routers);
    }
}
