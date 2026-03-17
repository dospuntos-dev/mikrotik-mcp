# Getting Started

This guide walks you through setting up mikrotik-mcp from scratch.

## 1. RouterOS Setup

### Enable the REST API

On your MikroTik router, enable the `www-ssl` service:

```routeros
/ip service enable www-ssl
```

If you don't have an SSL certificate, you can use the built-in self-signed one or generate a new one:

```routeros
/certificate add name=local-cert common-name=router key-size=2048
/certificate sign local-cert
/ip service set www-ssl certificate=local-cert
```

### Create an API User

Create a dedicated user for the MCP server:

```routeros
/user group add name=api policy=read,api,!ftp,!ssh,!reboot,!write,!policy,!test,!winbox,!password,!web,!sniff,!sensitive,!romon,!rest-api
/user add name=mcp-readonly group=api password=YOUR_STRONG_PASSWORD
```

> **Note:** The example above creates a **read-only** user. If you need write access (e.g., for `run_write_command`), add the `write` policy to the group. Use write access with caution.

### Repeat for Branch Routers

Each branch router that you want to manage needs:
1. REST API enabled (`www-ssl`)
2. The same API user created
3. Network reachability from the central router (via VPN tunnel)

## 2. Install .NET 10

Download and install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

Verify the installation:
```bash
dotnet --version
```

## 3. Clone and Configure

```bash
git clone https://github.com/dospuntos-dev/mikrotik-mcp.git
cd mikrotik-mcp
```

Create a local settings file (not tracked by git):
```bash
cp MikroTikMcp/appsettings.json MikroTikMcp/appsettings.Development.json
```

Edit `appsettings.Development.json` with your router details:
```json
{
  "MikroTik": {
    "Host": "192.168.88.1",
    "Port": 443,
    "Protocol": "https",
    "User": "mcp-readonly",
    "Password": "YOUR_STRONG_PASSWORD"
  },
  "RemoteRouters": {
    "User": "mcp-readonly",
    "Password": "YOUR_STRONG_PASSWORD",
    "Port": 443,
    "Protocol": "https"
  }
}
```

## 4. Run

```bash
dotnet run --project MikroTikMcp
```

The server starts on `http://localhost:5151`. You should see log output confirming the connection.

## 5. Connect an MCP Client

### Claude Desktop

Add to your Claude Desktop MCP configuration:

```json
{
  "mcpServers": {
    "mikrotik": {
      "url": "http://localhost:5151/sse"
    }
  }
}
```

Restart Claude Desktop. You should now see MikroTik tools available.

### Other MCP Clients

Any MCP-compatible client can connect to `http://localhost:5151/sse` (SSE transport).

## 6. First Commands to Try

Once connected, try asking your AI assistant:

- "List all routers connected via VPN"
- "Show me the system info of the central router"
- "What are the firewall filter rules?"
- "Show me all DHCP leases"
- "What's the health status of all routers?"
- "List all interfaces and their status on [router-name]"
