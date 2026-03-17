using System.ComponentModel;
using System.Text.Json;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class SystemTools
{
    [McpServerTool(Name = "get_system_info", ReadOnly = true)]
    [Description("Get router identity, model, RouterOS version, uptime, CPU and memory usage")]
    public static async Task<string> GetSystemInfo(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var identity = await client.GetAsync("/system/identity", ct);
        var resource = await client.GetAsync("/system/resource", ct);
        return Json.Format(new { identity, resource });
    }

    [McpServerTool(Name = "get_system_clock", ReadOnly = true)]
    [Description("Get router clock settings (date, time, timezone)")]
    public static async Task<string> GetSystemClock(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/system/clock", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_system_health", ReadOnly = true)]
    [Description("Get hardware health: temperature, voltage, fan speed (if supported by hardware)")]
    public static async Task<string> GetSystemHealth(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/system/health", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_system_packages", ReadOnly = true)]
    [Description("List installed RouterOS packages and their versions")]
    public static async Task<string> GetSystemPackages(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/system/package", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_logs", ReadOnly = true)]
    [Description("Get system log entries. Returns the most recent entries.")]
    public static async Task<string> GetLogs(
        RouterClientFactory factory,
        [Description("Max number of entries to return (default 50)")] int limit = 50,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/log", ct);
        if (data.ValueKind == JsonValueKind.Array)
        {
            var items = data.EnumerateArray().ToList();
            var sliced = items.Skip(Math.Max(0, items.Count - Math.Max(1, limit))).ToList();
            return Json.Format(sliced);
        }
        return Json.Format(data);
    }
}
