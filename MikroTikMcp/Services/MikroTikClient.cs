using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MikroTikMcp.Services;

public sealed class MikroTikSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 443;
    public string Protocol { get; set; } = "https";
    public string User { get; set; } = "admin";
    public string Password { get; set; } = "";
}

public sealed class RemoteRouterSettings
{
    public string User { get; set; } = "admin";
    public string Password { get; set; } = "";
    public int Port { get; set; } = 443;
    public string Protocol { get; set; } = "https";
}

public sealed class MikroTikClient
{
    private readonly HttpClient _http;
    private readonly ILogger<MikroTikClient> _logger;
    private readonly string _baseUrl;
    private readonly string _routerHost;

    public MikroTikClient(HttpClient http, MikroTikSettings settings, ILogger<MikroTikClient> logger)
    {
        _http = http;
        _logger = logger;
        _routerHost = settings.Host;
        var isDefaultPort = (settings.Protocol == "https" && settings.Port == 443)
            || (settings.Protocol == "http" && settings.Port == 80);
        _baseUrl = isDefaultPort
            ? $"{settings.Protocol}://{settings.Host}/rest"
            : $"{settings.Protocol}://{settings.Host}:{settings.Port}/rest";

        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{settings.User}:{settings.Password}"));
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<JsonElement> GetAsync(string path, CancellationToken ct = default)
    {
        var normalizedPath = NormalizePath(path);
        var url = $"{_baseUrl}{normalizedPath}";
        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _http.GetAsync(url, ct);
            sw.Stop();
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "MikroTik API error: {Method} {Path} returned {StatusCode} in {ElapsedMs}ms — Router: {RouterHost}",
                    "GET", normalizedPath, (int)response.StatusCode, sw.ElapsedMilliseconds, _routerHost);
                throw new MikroTikException($"GET {path} failed ({response.StatusCode}): {body}");
            }

            var result = JsonSerializer.Deserialize<JsonElement>(body);
            var itemCount = result.ValueKind == JsonValueKind.Array ? result.GetArrayLength() : 1;

            _logger.LogInformation(
                "MikroTik API call: {Method} {Path} — {StatusCode} — {ItemCount} items — {ElapsedMs}ms — Router: {RouterHost}",
                "GET", normalizedPath, (int)response.StatusCode, itemCount, sw.ElapsedMilliseconds, _routerHost);

            return result;
        }
        catch (Exception ex) when (ex is not MikroTikException)
        {
            sw.Stop();
            _logger.LogError(ex,
                "MikroTik API failure: {Method} {Path} — {ElapsedMs}ms — Router: {RouterHost} — Error: {ErrorMessage}",
                "GET", normalizedPath, sw.ElapsedMilliseconds, _routerHost, ex.Message);
            throw;
        }
    }

    public async Task<JsonElement> PostAsync(
        string path, object payload, CancellationToken ct = default)
    {
        var normalizedPath = NormalizePath(path);
        var url = $"{_baseUrl}{normalizedPath}";
        var json = JsonSerializer.Serialize(payload);
        var sw = Stopwatch.StartNew();

        try
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content, ct);
            sw.Stop();
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "MikroTik API error: {Method} {Path} returned {StatusCode} in {ElapsedMs}ms — Router: {RouterHost} — Payload: {Payload}",
                    "POST", normalizedPath, (int)response.StatusCode, sw.ElapsedMilliseconds, _routerHost, json);
                throw new MikroTikException($"POST {path} failed ({response.StatusCode}): {body}");
            }

            _logger.LogInformation(
                "MikroTik API call: {Method} {Path} — {StatusCode} — {ElapsedMs}ms — Router: {RouterHost} — Payload: {Payload}",
                "POST", normalizedPath, (int)response.StatusCode, sw.ElapsedMilliseconds, _routerHost, json);

            return string.IsNullOrWhiteSpace(body)
                ? JsonSerializer.Deserialize<JsonElement>("{}")
                : JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch (Exception ex) when (ex is not MikroTikException)
        {
            sw.Stop();
            _logger.LogError(ex,
                "MikroTik API failure: {Method} {Path} — {ElapsedMs}ms — Router: {RouterHost} — Error: {ErrorMessage}",
                "POST", normalizedPath, sw.ElapsedMilliseconds, _routerHost, ex.Message);
            throw;
        }
    }

    private static string NormalizePath(string path) =>
        path.StartsWith('/') ? path : $"/{path}";
}

public sealed class MikroTikException : Exception
{
    public MikroTikException(string message) : base(message) { }
}
