[CmdletBinding()]
param()

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
        throw 'Docker Desktop is required to stop the local PostgreSQL container.'
    }
}

if ($dockerPath -ne 'docker')
{
    $dockerDirectory = Split-Path -Parent $dockerPath
    $env:PATH = $dockerDirectory + [IO.Path]::PathSeparator + $env:PATH
}

& $dockerPath compose --file $composeFile stop
if ($LASTEXITCODE -ne 0)
{
    throw 'The local PostgreSQL container did not stop successfully.'
}
