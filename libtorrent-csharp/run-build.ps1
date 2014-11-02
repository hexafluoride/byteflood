param (
    [string]$NuGetAPIKey       = $null,
    [string]$Task              = "Default",
    [string]$Version           = "0.0.0"
 )

$nuget = Join-Path $PSScriptRoot "tools/nuget.exe"
Start-Process -NoNewWindow -Wait $nuget "restore packages.config -PackagesDirectory packages"
Start-Process -NoNewWindow -Wait $nuget "restore Ragnar.sln -PackagesDirectory packages"

$params = @{
    "NuGet_API_Key" = $NuGetAPIKey
    "Version" = $Version
}

Import-Module .\packages\psake.4.3.2\tools\psake.psm1

$psake.use_exit_on_error = $true

Invoke-psake build.ps1 -framework '4.0' -parameters $params $Task

if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }