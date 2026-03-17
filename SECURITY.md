# Security Policy

## Reporting a Vulnerability

**Do not open a public issue for security vulnerabilities.**

Please use [GitHub's private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability) to report security issues.

We will respond within 7 days and work with you to understand and address the issue.

## Security Considerations

This tool manages network infrastructure. Please read the [Security Warning](README.md#-security-warning) in the README carefully before using it.

### Key Risks

- **Network data exposure to AI**: The RouterOS REST API exposes firewall rules, VPN configurations, ARP tables, DNS entries, DHCP leases, and routing tables. An AI agent reading this data may retain or expose sensitive network information in unexpected ways.
- **No endpoint authentication**: The MCP server has no authentication. Anyone who can reach it can control your routers.
- **Write access**: The `run_write_command` tool can modify router configuration.

### Recommendations

1. Never expose the MCP server to the public internet
2. Always run behind a VPN or firewall
3. Use a read-only RouterOS API user when possible
4. Review all AI-suggested changes before applying
