# IIS Deployment Guide

This guide explains how to deploy mikrotik-mcp to Windows Server with IIS.

## Prerequisites

- Windows Server 2019+ with IIS
- [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- IIS with ASP.NET Core Module (ANCM) v2

## Manual Deployment

### 1. Publish the Application

```bash
dotnet publish MikroTikMcp -c Release -o ./publish
```

### 2. Create IIS Site

Using PowerShell (run as Administrator):

```powershell
Import-Module WebAdministration

$siteName = "MikroTikMcp"
$poolName = "MikroTikMcp"
$physicalPath = "C:\inetpub\mikrotik-mcp"  # Change to your preferred path
$port = 5151  # Change to your preferred port

# Create directory
New-Item -Path $physicalPath -ItemType Directory -Force

# Create App Pool (No Managed Code)
New-WebAppPool -Name $poolName
Set-ItemProperty "IIS:\AppPools\$poolName" -Name managedRuntimeVersion -Value ""

# Create Website
New-Website -Name $siteName `
    -PhysicalPath $physicalPath `
    -ApplicationPool $poolName `
    -Port $port `
    -Force
```

### 3. Deploy Files

```powershell
Copy-Item -Path ".\publish\*" -Destination "C:\inetpub\mikrotik-mcp" -Recurse -Force
```

### 4. Configure Settings

Create `appsettings.Production.json` in the deployment directory with your router configuration. This file is not tracked by git.

### 5. Start the Site

```powershell
Start-WebAppPool -Name "MikroTikMcp"
Start-Website -Name "MikroTikMcp"
```

## GitHub Actions (Self-Hosted Runner)

If you want automated deployment, set up a self-hosted GitHub Actions runner on your Windows Server and create a workflow based on this template:

```yaml
name: Deploy to IIS

on:
  push:
    branches: [main]

env:
  DOTNET_NOLOGO: true

jobs:
  deploy:
    runs-on: self-hosted
    defaults:
      run:
        working-directory: ./MikroTikMcp

    steps:
      - uses: actions/checkout@v4

      - name: Build & Publish
        run: dotnet publish -c Release -o "${{ github.workspace }}/publish"

      - name: Stop IIS
        shell: powershell
        run: |
          Import-Module WebAdministration
          Stop-Website -Name "YOUR_SITE_NAME"
          Stop-WebAppPool -Name "YOUR_POOL_NAME"
          Start-Sleep -Seconds 3

      - name: Deploy
        shell: powershell
        run: |
          Copy-Item "${{ github.workspace }}/publish/*" "YOUR_DEPLOY_PATH" -Recurse -Force

      - name: Start IIS
        shell: powershell
        run: |
          Import-Module WebAdministration
          Start-WebAppPool -Name "YOUR_POOL_NAME"
          Start-Website -Name "YOUR_SITE_NAME"
```

Replace `YOUR_SITE_NAME`, `YOUR_POOL_NAME`, and `YOUR_DEPLOY_PATH` with your values.

## Troubleshooting

- **Check logs:** Look at `.\logs\stdout` in the deployment directory for startup errors
- **App pool crashes:** Ensure the .NET 10 Hosting Bundle is installed
- **Connection issues:** Verify the router is reachable from the server
