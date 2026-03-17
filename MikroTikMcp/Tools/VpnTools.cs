using System.ComponentModel;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class VpnTools
{
    [McpServerTool(Name = "get_vpn_status", ReadOnly = true)]
    [Description("Get VPN tunnel status: IPsec SAs, L2TP, WireGuard peers. Silently skips VPN types that are not configured.")]
    public static async Task<string> GetVpnStatus(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var results = new Dictionary<string, object?>();

        var paths = new (string Key, string Path)[]
        {
            ("ipsec_policies", "/ip/ipsec/policy"),
            ("ipsec_active_peers", "/ip/ipsec/active-peers"),
            ("l2tp_server", "/interface/l2tp-server/server"),
            ("wireguard_interfaces", "/interface/wireguard"),
            ("wireguard_peers", "/interface/wireguard/peers"),
        };

        foreach (var (key, path) in paths)
        {
            try
            {
                results[key] = await client.GetAsync(path, ct);
            }
            catch (MikroTikException)
            {
                // VPN type not configured — skip
            }
        }

        return Json.Format(results);
    }
}
