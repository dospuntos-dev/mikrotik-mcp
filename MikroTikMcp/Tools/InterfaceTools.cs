using System.ComponentModel;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class InterfaceTools
{
    [McpServerTool(Name = "get_interfaces", ReadOnly = true)]
    [Description("List all network interfaces with status, MAC address, speed and traffic counters")]
    public static async Task<string> GetInterfaces(
        RouterClientFactory factory,
        [Description("Filter by type: ethernet, bridge, vlan, wireless, bonding, pppoe-client")] string? type = null,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var path = type is not null ? $"/interface/{type}" : "/interface";
        var data = await client.GetAsync(path, ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_ip_addresses", ReadOnly = true)]
    [Description("List all IP addresses assigned to interfaces")]
    public static async Task<string> GetIpAddresses(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/address", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_bridge", ReadOnly = true)]
    [Description("Get bridge configuration, port membership and host table")]
    public static async Task<string> GetBridge(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var bridges = await client.GetAsync("/interface/bridge", ct);
        var ports = await client.GetAsync("/interface/bridge/port", ct);
        var hosts = await client.GetAsync("/interface/bridge/host", ct);
        return Json.Format(new { bridges, ports, hosts });
    }

    [McpServerTool(Name = "get_vlans", ReadOnly = true)]
    [Description("List VLAN interfaces and their configuration")]
    public static async Task<string> GetVlans(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/interface/vlan", ct);
        return Json.Format(data);
    }
}
