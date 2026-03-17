using System.Text.Json;

namespace MikroTikMcp.Tools;

internal static class Json
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string Format(object data) =>
        JsonSerializer.Serialize(data, Options);
}
