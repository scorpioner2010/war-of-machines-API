[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

function Read-KeyValueFile {
    param([string]$Path)

    $values = @{}
    if (-not (Test-Path -LiteralPath $Path)) {
        return $values
    }

    foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith('#')) {
            continue
        }

        $separator = $line.IndexOf('=')
        if ($separator -le 0) {
            continue
        }

        $key = $line.Substring(0, $separator).Trim()
        $value = $line.Substring($separator + 1).Trim()
        $values[$key] = $value
    }

    return $values
}

function New-LocalCredential {
    $bytes = New-Object byte[] 32
    $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $generator.GetBytes($bytes)
    }
    finally {
        $generator.Dispose()
    }

    return [System.BitConverter]::ToString($bytes).Replace('-', '')
}

function Write-Utf8NoBom {
    param(
        [string]$Path,
        [string]$Content
    )

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null

$securityEnvPath = Join-Path $outputDirectory 'local-security.env'
$clientEnvPath = Join-Path $outputDirectory 'client-transport.env'
$certificatePath = Join-Path $outputDirectory 'server-cert.pem'
$privateKeyPath = Join-Path $outputDirectory 'server-cert.key'
$existing = Read-KeyValueFile $securityEnvPath

$credential = $existing['WOM_SERVER_CREDENTIAL']
if ([string]::IsNullOrWhiteSpace($credential)) {
    $credential = New-LocalCredential
}

if (-not (Test-Path -LiteralPath $certificatePath) -or -not (Test-Path -LiteralPath $privateKeyPath)) {
    Remove-Item -LiteralPath $certificatePath, $privateKeyPath -Force -ErrorAction SilentlyContinue
    $dotnet = Get-Command dotnet.exe -ErrorAction Stop
    & $dotnet.Source dev-certs https --export-path $certificatePath --format Pem --no-password --quiet | Out-Null
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $certificatePath) -or -not (Test-Path -LiteralPath $privateKeyPath)) {
        throw 'Could not create the local PEM development certificate.'
    }
}

$dotnet = Get-Command dotnet.exe -ErrorAction Stop
& $dotnet.Source dev-certs https --trust --quiet | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw 'Could not trust the local HTTPS development certificate.'
}

$securityContent = @(
    'WOM_API_BASE=https://localhost:7216'
    'WOM_SERVER_ID=local-server-1'
    "WOM_SERVER_CREDENTIAL=$credential"
    'WOM_SERVER_REGION=local'
    'WOM_SERVER_ADDRESS=127.0.0.1'
    'WOM_SERVER_PORT=7770'
    'DedicatedServers__Provisioned__0__ServerId=local-server-1'
    "DedicatedServers__Provisioned__0__Credential=$credential"
    "WOM_UTP_SERVER_CERTIFICATE=$certificatePath"
    "WOM_UTP_SERVER_PRIVATE_KEY=$privateKeyPath"
    'WOM_UTP_SERVER_COMMON_NAME=localhost'
    "WOM_UTP_CA_CERTIFICATE=$certificatePath"
) -join [Environment]::NewLine

$clientContent = @(
    'WOM_API_BASE=https://localhost:7216'
    'WOM_UTP_SERVER_COMMON_NAME=localhost'
    "WOM_UTP_CA_CERTIFICATE=$certificatePath"
) -join [Environment]::NewLine

Write-Utf8NoBom $securityEnvPath $securityContent
Write-Utf8NoBom $clientEnvPath $clientContent
Write-Output "LOCAL_SECURITY_ENV=$securityEnvPath"
Write-Output "CLIENT_TRANSPORT_ENV=$clientEnvPath"
