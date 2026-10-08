[CmdletBinding()]
param(
    [switch]$SkipDatabaseStart
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $projectRoot 'docker-compose.local.yml'
$dockerPath = 'docker'

if ((Get-Command docker -ErrorAction SilentlyContinue) -eq $null)
{
    $userDockerPath = Join-Path $env:LOCALAPPDATA 'Programs\DockerDesktop\resources\bin\docker.exe'
    if (Test-Path -LiteralPath $userDockerPath)
    {
        $dockerPath = $userDockerPath
    }
    else
    {
        throw 'Docker Desktop is required. Install and start Docker Desktop, then run this script again.'
    }
}

if ($dockerPath -ne 'docker')
{
    $dockerDirectory = Split-Path -Parent $dockerPath
    $env:PATH = $dockerDirectory + [IO.Path]::PathSeparator + $env:PATH
}

if ($SkipDatabaseStart -eq $false)
{
    & $dockerPath compose --file $composeFile up --detach --wait
    if ($LASTEXITCODE -ne 0)
    {
        throw 'The local PostgreSQL container did not start successfully.'
    }
}

$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DATABASE_URL = 'Host=127.0.0.1;Port=5433;Database=war_of_machines_local;Username=wom_local;Password=wom_local_dev_password'
$env:ConnectionStrings__DefaultConnection = ''

Push-Location $projectRoot
try
{
    & dotnet run --launch-profile https
    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}
finally
{
    Pop-Location
}
