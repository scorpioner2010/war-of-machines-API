[CmdletBinding()]
param(
    [string]$UnityBuildId = 'RAVServer-20261122-staging-hostallowlist',
    [string]$UnityBuildPath = 'D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-staging-hostallowlist',
    [string]$UnityPayloadPath = (Join-Path $PSScriptRoot 'payload\unity\RAVServer-20261122-staging-hostallowlist'),
    [string]$ApiSourcePath = (Join-Path $PSScriptRoot '..\..\RavApi')
)

$ErrorActionPreference = 'Stop'
$stagingRoot = [IO.Path]::GetFullPath($PSScriptRoot)
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $stagingRoot 'artifacts'))
$unityBuild = [IO.Path]::GetFullPath($UnityBuildPath)
$unityPayload = [IO.Path]::GetFullPath($UnityPayloadPath)
$apiSource = [IO.Path]::GetFullPath($ApiSourcePath)
$apiSnapshot = [IO.Path]::GetFullPath((Join-Path $stagingRoot 'api\RavApi'))
$repoRoot = [IO.Path]::GetFullPath((Join-Path $stagingRoot '..\..'))
$tar = Get-Command tar.exe -ErrorAction Stop | Select-Object -ExpandProperty Source

function Assert-ChildPath([string]$Path, [string]$Parent) {
    $prefix = [IO.Path]::GetFullPath($Parent).TrimEnd([char[]]@(92,47)) + [IO.Path]::DirectorySeparatorChar
    $full = [IO.Path]::GetFullPath($Path)
    if (-not $full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing an operation outside the expected directory: $full"
    }
}

function Get-TreeHashes([string]$Root) {
    $rootPrefix = [IO.Path]::GetFullPath($Root).TrimEnd([char[]]@(92,47)) + [IO.Path]::DirectorySeparatorChar
    $map = [Collections.Generic.SortedDictionary[string,string]]::new([StringComparer]::Ordinal)
    Get-ChildItem -LiteralPath $Root -Directory -Recurse -Force | ForEach-Object {
        if ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Refusing symlink/reparse point in artifact tree: $($_.FullName)"
        }
    }
    Get-ChildItem -LiteralPath $Root -File -Recurse -Force | Where-Object {
        $_.FullName -notmatch '[\\/]+RAV_BurstDebugInformation_DoNotShip([\\/]|$)'
    } | ForEach-Object {
        if ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Refusing symlink/reparse point in artifact tree: $($_.FullName)"
        }
        $relative = $_.FullName.Substring($rootPrefix.Length).Replace('\', '/')
        $map.Add($relative, (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant())
    }
    return ,$map
}

if (-not (Test-Path -LiteralPath $unityBuild -PathType Container)) { throw "Unity build source not found: $unityBuild" }
if (-not (Test-Path -LiteralPath $unityPayload -PathType Container)) { throw "Verified staging Unity payload not found: $unityPayload" }
if (-not (Test-Path -LiteralPath $apiSource -PathType Container) -or -not (Test-Path -LiteralPath $apiSnapshot -PathType Container)) { throw 'API source or staging API snapshot is missing.' }
if ($UnityBuildId -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$' -or $UnityBuildId -in @('.', '..')) { throw 'UnityBuildId must be a single safe release identifier.' }
foreach ($root in @($unityBuild, $unityPayload)) {
    foreach ($required in @('RAVServer.x86_64', 'UnityPlayer.so', 'RAVServer_Data/globalgamemanagers', 'RAVServer_Data/Managed/Assembly-CSharp.dll', 'RAVServer_Data/MonoBleedingEdge/x86_64/libmonobdwgc-2.0.so')) {
        if (-not (Test-Path -LiteralPath (Join-Path $root $required) -PathType Leaf)) { throw "Unity build is incomplete ($required): $root" }
    }
}
if (Test-Path -LiteralPath (Join-Path $unityPayload 'RAV_BurstDebugInformation_DoNotShip')) {
    throw 'The staging Unity payload must exclude RAV_BurstDebugInformation_DoNotShip.'
}
$unityExecutable = Join-Path $unityBuild 'RAVServer.x86_64'
$elfHeader = [IO.File]::ReadAllBytes($unityExecutable)
if ($elfHeader.Length -lt 20 -or [BitConverter]::ToString($elfHeader, 0, 4) -ne '7F-45-4C-46' -or $elfHeader[4] -ne 2 -or $elfHeader[5] -ne 1 -or [BitConverter]::ToUInt16($elfHeader, 18) -ne 62) {
    throw 'Unity server executable must be a little-endian ELF64 x86_64 binary.'
}
$assemblyText = [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes((Join-Path $unityBuild 'RAVServer_Data/Managed/Assembly-CSharp.dll')))
if (-not $assemblyText.Contains('IsExplicitStagingBase')) {
    throw 'Unity build lacks the staging-only API hostname allowlist fix; refusing to package it.'
}

$sourceHashes = Get-TreeHashes $unityBuild
$payloadHashes = Get-TreeHashes $unityPayload
if ($sourceHashes.Count -ne 189 -or $payloadHashes.Count -ne $sourceHashes.Count) {
    throw "Unity build file count differs from the verified build record (expected 189; source=$($sourceHashes.Count), payload=$($payloadHashes.Count))."
}
foreach ($relative in $sourceHashes.Keys) {
    if (-not $payloadHashes.ContainsKey($relative) -or $payloadHashes[$relative] -ne $sourceHashes[$relative]) {
        throw "Unity source/payload SHA-256 mismatch: $relative"
    }
}
$apiSourceFiles = Get-ChildItem -LiteralPath $apiSource -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Extension -in @('.cs', '.csproj')
}
$apiSnapshotFiles = Get-ChildItem -LiteralPath $apiSnapshot -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Extension -in @('.cs', '.csproj')
}
if ($apiSourceFiles.Count -ne $apiSnapshotFiles.Count) { throw "API staging snapshot file count differs from source (source=$($apiSourceFiles.Count), snapshot=$($apiSnapshotFiles.Count))." }
foreach ($sourceFile in $apiSourceFiles) {
    $relative = $sourceFile.FullName.Substring($apiSource.TrimEnd([char[]]@(92,47)).Length + 1)
    $snapshotFile = Join-Path $apiSnapshot $relative
    if (-not (Test-Path -LiteralPath $snapshotFile -PathType Leaf)) { throw "API staging snapshot is missing: $relative" }
    $sourceHash = (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash
    $snapshotHash = (Get-FileHash -LiteralPath $snapshotFile -Algorithm SHA256).Hash
    if ($sourceHash -ne $snapshotHash) { throw "API source/snapshot SHA-256 mismatch: $relative" }
}

New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
$bundleId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ', [Globalization.CultureInfo]::InvariantCulture)
$bundleName = "RAV-Staging-3B-$bundleId"
$workRoot = Join-Path $artifactRoot ".work-$bundleId"
$verifyRoot = Join-Path $artifactRoot ".verify-$bundleId"
$packageRoot = Join-Path $workRoot 'rav-staging'
$archivePath = Join-Path $artifactRoot "$bundleName.tar.gz"
$hashPath = "$archivePath.sha256"
Assert-ChildPath $workRoot $artifactRoot
Assert-ChildPath $verifyRoot $artifactRoot
Assert-ChildPath $archivePath $artifactRoot
if ((Test-Path -LiteralPath $workRoot) -or (Test-Path -LiteralPath $verifyRoot) -or (Test-Path -LiteralPath $archivePath) -or (Test-Path -LiteralPath $hashPath)) {
    throw "Bundle output already exists; refusing to overwrite: $bundleName"
}

$completed = $false
try {
    New-Item -ItemType Directory -Path $packageRoot | Out-Null
    $excluded = @('artifacts', 'payload', 'bundle')
    Get-ChildItem -LiteralPath $stagingRoot -File -Recurse -Force | ForEach-Object {
        $relative = $_.FullName.Substring($stagingRoot.TrimEnd([char[]]@(92,47)).Length + 1)
        $segments = $relative -split '[\\/]'
        if ($segments.Count -gt 0 -and $excluded -contains $segments[0]) { return }
        if ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Refusing symlink/reparse point: $($_.FullName)" }
        $destination = Join-Path $packageRoot $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination
    }

    $buildDestination = Join-Path $packageRoot ("unity/build/$UnityBuildId")
    New-Item -ItemType Directory -Path (Split-Path -Parent $buildDestination) -Force | Out-Null
    Copy-Item -LiteralPath $unityPayload -Destination $buildDestination -Recurse
    $payloadInBundle = Get-TreeHashes $buildDestination
    if ($payloadInBundle.Count -ne $sourceHashes.Count) { throw 'Packaged Unity build file count changed during copy.' }
    foreach ($relative in $sourceHashes.Keys) {
        if ($payloadInBundle[$relative] -ne $sourceHashes[$relative]) { throw "Packaged Unity build hash mismatch: $relative" }
    }

    $bytes = (Get-ChildItem -LiteralPath $buildDestination -File -Recurse | Measure-Object -Property Length -Sum).Sum
    $unityArchive = "$buildDestination.tar.gz"
    & $tar -czf $unityArchive -C $buildDestination .
    if ($LASTEXITCODE -ne 0) { throw "Unity release tar creation failed with exit code $LASTEXITCODE." }
    $unityVerifyRoot = Join-Path $workRoot '.unity-verify'
    New-Item -ItemType Directory -Path $unityVerifyRoot | Out-Null
    & $tar -xzf $unityArchive -C $unityVerifyRoot
    if ($LASTEXITCODE -ne 0) { throw "Unity release tar extraction validation failed with exit code $LASTEXITCODE." }
    $unityArchiveHashes = Get-TreeHashes $unityVerifyRoot
    if ($unityArchiveHashes.Count -ne $sourceHashes.Count) { throw 'Unity release archive file count differs from the source build.' }
    foreach ($relative in $sourceHashes.Keys) {
        if ($unityArchiveHashes[$relative] -ne $sourceHashes[$relative]) { throw "Unity release archive SHA-256 mismatch: $relative" }
    }
    Assert-ChildPath $buildDestination $packageRoot
    Remove-Item -LiteralPath $buildDestination -Recurse -Force
    Remove-Item -LiteralPath $unityVerifyRoot -Recurse -Force

    $info = @(
        'RAV Staging Stage 3B deployment bundle',
        "Created UTC: $([DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ', [Globalization.CultureInfo]::InvariantCulture))",
        "Unity release archive: unity/build/$UnityBuildId.tar.gz",
        "Unity files: $($sourceHashes.Count)",
        "Unity bytes: $bytes",
        "API source/project files: $($apiSourceFiles.Count) (SHA-256 matched)",
        "Unity source: $([IO.Path]::GetFileName($unityBuild))",
        'Secrets: none; generate on the isolated VPS during bootstrap.',
        'Runtime deployment: NOT STARTED'
    ) -join "`n"
    [IO.File]::WriteAllText((Join-Path $packageRoot 'BUNDLE-INFO.txt'), $info + "`n", [Text.UTF8Encoding]::new($false))

    # The Windows staging copy can have CRLF checkout endings; ship POSIX shell scripts as LF.
    Get-ChildItem -LiteralPath $packageRoot -Filter '*.sh' -File -Recurse | ForEach-Object {
        $text = [IO.File]::ReadAllText($_.FullName)
        $text = $text -replace "`r`n?", "`n"
        [IO.File]::WriteAllText($_.FullName, $text, [Text.UTF8Encoding]::new($false))
        if (-not $text.StartsWith('#!/usr/bin/env bash')) { throw "Missing Bash shebang: $($_.FullName)" }
        if ($text -match "`r" -or $text -match '(?m)[ \t]+$') { throw "Shell script contains CR or trailing whitespace: $($_.FullName)" }
    }

    $manifestLines = [Collections.Generic.List[string]]::new()
    $allFiles = Get-TreeHashes $packageRoot
    foreach ($relative in $allFiles.Keys) { $manifestLines.Add("$($allFiles[$relative])  $relative") }
    [IO.File]::WriteAllLines((Join-Path $packageRoot 'MANIFEST.sha256'), $manifestLines, [Text.UTF8Encoding]::new($false))

    & $tar -czf $archivePath -C $workRoot 'rav-staging'
    if ($LASTEXITCODE -ne 0) { throw "tar.exe creation failed with exit code $LASTEXITCODE." }
    $entries = & $tar -tzf $archivePath
    if ($LASTEXITCODE -ne 0 -or -not ($entries -contains 'rav-staging/MANIFEST.sha256')) { throw 'Archive listing validation failed.' }

    New-Item -ItemType Directory -Path $verifyRoot | Out-Null
    & $tar -xzf $archivePath -C $verifyRoot
    if ($LASTEXITCODE -ne 0) { throw "tar.exe extraction validation failed with exit code $LASTEXITCODE." }
    $verifiedRoot = Join-Path $verifyRoot 'rav-staging'
    foreach ($line in [IO.File]::ReadAllLines((Join-Path $verifiedRoot 'MANIFEST.sha256'))) {
        if ($line -notmatch '^([0-9a-f]{64})  (.+)$') { throw "Malformed manifest entry: $line" }
        $expected = $Matches[1]
        $relative = $Matches[2].Replace('/', [IO.Path]::DirectorySeparatorChar)
        $filePath = Join-Path $verifiedRoot $relative
        Assert-ChildPath $filePath $verifiedRoot
        if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) { throw "Manifest file missing after extraction: $relative" }
        $actual = (Get-FileHash -LiteralPath $filePath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne $expected) { throw "Archive SHA-256 verification failed: $relative" }
    }
    $archiveHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText($hashPath, "$archiveHash  $([IO.Path]::GetFileName($archivePath))`n", [Text.UTF8Encoding]::new($false))
    $completed = $true
    Write-Output "Bundle: $archivePath"
    Write-Output "SHA-256: $archiveHash"
    Write-Output "Unity payload: $($sourceHashes.Count) files; $bytes bytes; source and packaged hashes match."
}
finally {
    foreach ($temporaryPath in @($workRoot, $verifyRoot)) {
        if (Test-Path -LiteralPath $temporaryPath) {
            Assert-ChildPath $temporaryPath $artifactRoot
            Remove-Item -LiteralPath $temporaryPath -Recurse -Force
        }
    }
    if (-not $completed) {
        foreach ($partialPath in @($archivePath, $hashPath)) {
            if (Test-Path -LiteralPath $partialPath) {
                Assert-ChildPath $partialPath $artifactRoot
                Remove-Item -LiteralPath $partialPath -Force
            }
        }
    }
}
