$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
$StubsPrebuilt = Join-Path $RepoRoot 'stubs/prebuilt'
$LibsDir = Join-Path $StubsPrebuilt 'Libraries'
$RmlLibsDir = Join-Path $StubsPrebuilt 'rml_libs'

New-Item -ItemType Directory -Force -Path $LibsDir | Out-Null
New-Item -ItemType Directory -Force -Path $RmlLibsDir | Out-Null

$ResoniteModLoaderPackageId = 'ResoniteModLoader'
$ResoniteModLoaderPackageVersion = '5.0.1'
$ResoniteModLoaderTarget = 'net10.0'
$ResoniteModLoaderDll = Join-Path $LibsDir 'ResoniteModLoader.dll'

$LibHarmonyPackageId = 'Lib.Harmony'
$LibHarmonyPackageVersion = '2.4.2'
$LibHarmonyTarget = 'net10.0'
$HarmonyDll = Join-Path $RmlLibsDir '0Harmony.dll'

function Ensure-NuGetDll {
  param(
    [string]$PackageId,
    [string]$Version,
    [string]$TargetFramework,
    [string]$OutDllPath,
    [string]$PackageDllPath
  )

  if (Test-Path $OutDllPath) {
    return
  }

  Write-Host "Bootstrapping $PackageId $Version for $TargetFramework"

  $probeDir = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString())
  New-Item -ItemType Directory -Force -Path $probeDir | Out-Null

  dotnet new console -n Probe -o $probeDir --framework $TargetFramework | Out-Null
  dotnet add (Join-Path $probeDir 'Probe.csproj') package $PackageId --version $Version | Out-Null
  dotnet restore (Join-Path $probeDir 'Probe.csproj') -p:RestoreIgnoreFailedSources=true | Out-Null

  # Copy from known global-packages location.
  # Example path: ~/.nuget/packages/<lowercase-id>/<version>/lib/<tfm>/<dll>
  $homeDir = $env:HOME
  $dllCandidate = $PackageDllPath.Replace('{HOME}', $homeDir)
  $dllCandidate = $dllCandidate.Replace('{0}', $Version).Replace('{1}', $TargetFramework)
  if (!(Test-Path $dllCandidate)) {
    throw "Could not find expected DLL at $dllCandidate"
  }
  Copy-Item -Force -Path $dllCandidate -Destination $OutDllPath
}

Ensure-NuGetDll -PackageId $ResoniteModLoaderPackageId -Version $ResoniteModLoaderPackageVersion -TargetFramework $ResoniteModLoaderTarget -OutDllPath $ResoniteModLoaderDll `
  -PackageDllPath '{HOME}/.nuget/packages/resonitemodloader/{0}/lib/{1}/ResoniteModLoader.dll' -f $ResoniteModLoaderPackageVersion, $ResoniteModLoaderTarget

Ensure-NuGetDll -PackageId $LibHarmonyPackageId -Version $LibHarmonyPackageVersion -TargetFramework $LibHarmonyTarget -OutDllPath $HarmonyDll `
  -PackageDllPath '{HOME}/.nuget/packages/lib.harmony/{0}/lib/{1}/0Harmony.dll' -f $LibHarmonyPackageVersion, $LibHarmonyTarget

Write-Host "Deps ready:"
Write-Host "- $ResoniteModLoaderDll"
Write-Host "- $HarmonyDll"
