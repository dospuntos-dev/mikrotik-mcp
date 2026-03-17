# Contributing to mikrotik-mcp

Thank you for your interest in contributing! This project is in its early stages and we welcome all forms of help.

## How to Contribute

### Reporting Bugs

- Use [GitHub Issues](../../issues) to report bugs
- Include your RouterOS version, .NET version, and steps to reproduce
- Check existing issues before creating a new one

### Suggesting Features

- Open a [Feature Request](../../issues/new?template=feature_request.md)
- Describe the use case, not just the solution
- Check the [Roadmap](README.md#roadmap) to see if it's already planned

### Submitting Changes

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Make your changes
4. Ensure the project builds: `dotnet build`
5. Commit your changes with a clear message
6. Push to your fork and open a Pull Request

### Code Style

- Follow standard .NET conventions
- Nullable reference types are enabled — handle nulls explicitly
- Keep tools focused: one MCP tool per RouterOS API endpoint
- Use `[McpServerTool]` attributes with clear `Name` and `Description`
- Mark read-only tools with `ReadOnly = true`, destructive ones with `Destructive = true`

### Areas Where Help is Needed

- **Tests** — The project currently has no tests. Unit and integration tests are very welcome.
- **Docker** — A Dockerfile for easier deployment would be valuable.
- **Security** — Authentication, read-only mode, audit logging (see Security First roadmap).
- **Documentation** — Better getting started guides, tool examples, RouterOS setup guides.

## License

By contributing, you agree that your contributions will be licensed under the [AGPL-3.0 License](LICENSE).
