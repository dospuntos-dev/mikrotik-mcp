using System.ComponentModel;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class RoutingTools
{
    [McpServerTool(Name = "get_routes", ReadOnly = true)]
    [Description("Get IP routing table (static, connected, dynamic routes)")]
    public static async Task<string> GetRoutes(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/route", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_arp_table", ReadOnly = true)]
    [Description("List ARP entries (IP to MAC address mappings)")]
    public static async Task<string> GetArpTable(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/arp", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_dns", ReadOnly = true)]
    [Description("Get DNS server configuration, cache settings and static entries")]
    public static async Task<string> GetDns(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var config = await client.GetAsync("/ip/dns", ct);
        var staticEntries = await client.GetAsync("/ip/dns/static", ct);
        return Json.Format(new { config, static_entries = staticEntries });
    }

    [McpServerTool(Name = "get_dhcp_leases", ReadOnly = true)]
    [Description("List DHCP server leases with IP, MAC, hostname and status")]
    public static async Task<string> GetDhcpLeases(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/dhcp-server/lease", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_dhcp_server", ReadOnly = true)]
    [Description("Get DHCP server configuration and networks")]
    public static async Task<string> GetDhcpServer(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var servers = await client.GetAsync("/ip/dhcp-server", ct);
        var networks = await client.GetAsync("/ip/dhcp-server/network", ct);
        return Json.Format(new { servers, networks });
    }

    [McpServerTool(Name = "get_ip_pool", ReadOnly = true)]
    [Description("List IP pools used by DHCP and other services")]
    public static async Task<string> GetIpPool(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/ip/pool", ct);
        return Json.Format(data);
    }
}
