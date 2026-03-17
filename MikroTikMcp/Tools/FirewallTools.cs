using System.ComponentModel;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class FirewallTools
{
    [McpServerTool(Name = "get_firewall_filter", ReadOnly = true)]
    [Description("List firewall filter rules (input, forward, output chains)")]
    public static async Task<string> GetFirewallFilter(
        RouterClientFactory factory,
        [Description("Filter by chain: input, forward, output")] string? chain = null,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var path = chain is not null
            ? $"/ip/firewall/filter?chain={chain}"
            : "/ip/firewall/filter";
        var data = await client.GetAsync(path, ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_firewall_nat", ReadOnly = true)]
    [Description("List NAT rules (srcnat, dstnat masquerade, port forwarding)")]
    public static async Task<string> GetFirewallNat(
        RouterClientFactory factory,
        [Description("Filter by chain: srcnat, dstnat")] string? chain = null,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var path = chain is not null
            ? $"/ip/firewall/nat?chain={chain}"
            : "/ip/firewall/nat";
        var data = await client.GetAsync(path, ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_firewall_mangle", ReadOnly = true)]
    [Description("List mangle rules (packet marking, QoS)")]
    public static async Task<string> GetFirewallMangle(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/firewall/mangle", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_firewall_address_lists", ReadOnly = true)]
    [Description("List firewall address lists (used in rules for grouping IPs)")]
    public static async Task<string> GetFirewallAddressLists(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/firewall/address-list", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_firewall_connections", ReadOnly = true)]
    [Description("List active firewall connection tracking entries")]
    public static async Task<string> GetFirewallConnections(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/firewall/connection", ct);
        return Json.Format(data);
    }
}
