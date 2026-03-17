using System.ComponentModel;
using MikroTikMcp.Services;
using ModelContextProtocol.Server;

namespace MikroTikMcp.Tools;

[McpServerToolType]
public sealed class QueueTools
{
    [McpServerTool(Name = "get_simple_queues", ReadOnly = true)]
    [Description("List simple queues (bandwidth limits per target IP/subnet)")]
    public static async Task<string> GetSimpleQueues(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/queue/simple", ct);
        return Json.Format(data);
    }

    [McpServerTool(Name = "get_queue_tree", ReadOnly = true)]
    [Description("List queue tree entries (hierarchical bandwidth management)")]
    public static async Task<string> GetQueueTree(
        RouterClientFactory factory,
        [Description("Router name or tunnel IP (omit for central)")] string? router = null,
        CancellationToken ct = default)
    {
        var client = await factory.GetClientAsync(router, ct);
        var data = await client.GetAsync("/queue/tree", ct);
        return Json.Format(data);
    }
}
