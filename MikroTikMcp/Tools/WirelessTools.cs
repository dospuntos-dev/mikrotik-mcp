using System.ComponentModel;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class WirelessTools
{
    [McpServerTool(Name = "get_wireless_interfaces", ReadOnly = true)]
    [Description("List wireless interfaces and their configuration (SSID, frequency, band)")]
    public static async Task<string> GetWirelessInterfaces(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/interface/wireless", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_wireless_clients", ReadOnly = true)]
    [Description("List connected wireless clients (registration table) with signal strength and data rates")]
    public static async Task<string> GetWirelessClients(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/interface/wireless/registration-table", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_wireless_security", ReadOnly = true)]
    [Description("List wireless security profiles")]
    public static async Task<string> GetWirelessSecurity(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/interface/wireless/security-profiles", ct);
        return Json.Format(data);
    }
}
