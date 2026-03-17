using System.ComponentModel;
using System.Text.Json;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class GenericTools
{
    [McpServerTool(Name = "run_command", ReadOnly = true)]
    [Description("Execute an arbitrary read-only GET query against the RouterOS REST API. Path maps to RouterOS menu structure, e.g. '/system/clock', '/ip/pool', '/interface/ethernet', '/caps-man/registration-table'")]
    public static async Task<string> RunCommand(
        RouterClientFactory factory,
        [Description("REST API path, e.g. '/system/clock', '/ip/pool', '/routing/ospf/neighbor'")] string path,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        try
        {
            var client = await factory.GetClientAsync(router, ct);
            var data = await client.GetAsync(path, ct);
            return Json.Format(data);
        }
        catch (Exception ex)
        {
            return $"ERROR [{router ?? "central"}]: {ex.GetType().Name}: {ex.Message}";
        }
    }

    [McpServerTool(Name = "run_write_command", Destructive = true)]
    [Description("Execute a POST (write) operation against the RouterOS REST API. Use with caution — this can modify router configuration. Examples: add firewall rule, enable/disable interface, add DNS entry")]
    public static async Task<string> RunWriteCommand(
        RouterClientFactory factory,
        [Description("REST API path, e.g. '/ip/firewall/filter/add', '/interface/disable'")] string path,
        [Description("JSON body as string, e.g. '{\"address\":\"10.0.0.0/24\",\"interface\":\"ether1\"}'")] string body,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        try
        {
            var client = await factory.GetClientAsync(router, ct);
            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(body)
                ?? throw new ArgumentException("Invalid JSON body");
            var data = await client.PostAsync(path, payload, ct);
            return Json.Format(data);
        }
        catch (Exception ex)
        {
            return $"ERROR [{router ?? "central"}]: {ex.GetType().Name}: {ex.Message}";
        }
    }
}
