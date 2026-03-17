# Tools Reference

Complete reference for all MCP tools provided by mikrotik-mcp.

All tools accept an optional `router` parameter (name or IP) to target a specific remote router. If omitted, the command runs on the central router.

---

## System Tools

### `get_system_info`
Returns general system information: board name, architecture, RouterOS version, CPU, memory, uptime, etc.
- **Read-only:** Yes

### `get_system_health`
Returns hardware health data: temperature, voltage, fan speed (when available).
- **Read-only:** Yes

### `get_logs`
Returns recent system log entries.
- **Read-only:** Yes

### `get_system_packages`
Lists installed RouterOS packages and their versions.
- **Read-only:** Yes

### `get_system_clock`
Returns the router's current date, time, and timezone.
- **Read-only:** Yes

---

## Interface Tools

### `get_interfaces`
Lists all network interfaces with their type, status, and traffic counters.
- **Read-only:** Yes

### `get_ip_addresses`
Lists all IP addresses assigned to interfaces.
- **Read-only:** Yes

### `get_bridge`
Returns bridge configuration and ports.
- **Read-only:** Yes

### `get_vlans`
Lists VLAN configurations.
- **Read-only:** Yes

---

## Firewall Tools

### `get_firewall_filter`
Returns all firewall filter rules (input, forward, output chains).
- **Read-only:** Yes

### `get_firewall_nat`
Returns all NAT rules (srcnat, dstnat).
- **Read-only:** Yes

### `get_firewall_mangle`
Returns all mangle rules.
- **Read-only:** Yes

### `get_firewall_address_lists`
Returns all address list entries.
- **Read-only:** Yes

### `get_firewall_connections`
Returns active connection tracking entries.
- **Read-only:** Yes

---

## Routing Tools

### `get_routes`
Returns the routing table.
- **Read-only:** Yes

### `get_arp_table`
Returns the ARP table.
- **Read-only:** Yes

### `get_dns`
Returns DNS configuration and static entries.
- **Read-only:** Yes

### `get_dhcp_server`
Returns DHCP server configuration.
- **Read-only:** Yes

### `get_dhcp_leases`
Returns active DHCP leases.
- **Read-only:** Yes

### `get_ip_pool`
Returns IP pool configurations.
- **Read-only:** Yes

---

## VPN Tools

### `get_vpn_status`
Returns comprehensive VPN status: WireGuard peers, PPP active connections, IPsec SAs, and L2TP status.
- **Read-only:** Yes

---

## Wireless Tools

### `get_wireless_interfaces`
Lists wireless interfaces and their configuration.
- **Read-only:** Yes

### `get_wireless_clients`
Returns connected wireless clients (registration table).
- **Read-only:** Yes

### `get_wireless_security`
Returns wireless security profiles.
- **Read-only:** Yes

---

## Queue Tools

### `get_simple_queues`
Returns simple queue configurations and statistics.
- **Read-only:** Yes

### `get_queue_tree`
Returns queue tree configurations.
- **Read-only:** Yes

---

## Generic Tools

### `run_command`
Execute any read-only REST API command. Pass a RouterOS REST API path.
- **Read-only:** Yes
- **Parameters:**
  - `command` (required): REST API path, e.g., `/ip/address`

### `run_write_command`
Execute a write REST API command (POST). Can modify router configuration.
- **Read-only:** No
- **Destructive:** Yes
- **Parameters:**
  - `command` (required): REST API path
  - `body` (optional): JSON body for the request

---

## Discovery Tools

### `list_routers`
Lists all reachable remote routers discovered through active VPN connections (PPP and WireGuard).
- **Read-only:** Yes
