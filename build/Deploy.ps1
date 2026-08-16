param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$coreProject = Join-Path $projectRoot 'src\ItemSpawnerEnhanced\ItemSpawnerEnhanced.csproj'
$chineseProject = Join-Path $projectRoot 'src\ItemSpawnerEnhanced.ChineseSearch\ItemSpawnerEnhanced.ChineseSearch.csproj'

if (-not $SkipBuild) {
    & dotnet build $chineseProject -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) { throw 'ItemSpawnerEnhanced build failed.' }
}

$propertiesOutput = (& dotnet msbuild $coreProject -nologo `
    -getProperty:PeakGameDir `
    -getProperty:BepInExCoreDir) -join [Environment]::NewLine
if ($LASTEXITCODE -ne 0) { throw 'Could not read PEAK build properties.' }
$jsonStart = $propertiesOutput.IndexOf('{')
if ($jsonStart -lt 0) { throw 'MSBuild did not return PEAK build properties as JSON.' }
$properties = ($propertiesOutput.Substring($jsonStart) | ConvertFrom-Json).Properties
if ([string]::IsNullOrWhiteSpace($properties.PeakGameDir)) {
    throw 'Set PEAK_GAME_DIR or copy PeakGameDir.props.example to PeakGameDir.props.'
}
if (-not (Test-Path -LiteralPath $properties.BepInExCoreDir)) {
    throw "BepInEx core directory was not found: $($properties.BepInExCoreDir)"
}

$coreOutputDir = Join-Path $projectRoot "src\ItemSpawnerEnhanced\bin\$Configuration"
$chineseOutputDir = Join-Path $projectRoot "src\ItemSpawnerEnhanced.ChineseSearch\bin\$Configuration"
$assemblies = @(
    (Join-Path $coreOutputDir 'ItemSpawnerEnhanced.dll'),
    (Join-Path $chineseOutputDir 'ItemSpawnerEnhanced.ChineseSearch.dll')
)
foreach ($assembly in $assemblies) {
    if (-not (Test-Path -LiteralPath $assembly)) { throw "Required output was not found: $assembly" }
}

$deployDir = Join-Path $properties.PeakGameDir 'BepInEx\plugins\ItemSpawnerEnhanced'
New-Item -ItemType Directory -Path $deployDir -Force | Out-Null
Copy-Item -LiteralPath $assemblies -Destination $deployDir -Force
$legacyPinyinAssembly = Join-Path $deployDir 'TinyPinyin.dll'
if (Test-Path -LiteralPath $legacyPinyinAssembly) {
    Remove-Item -LiteralPath $legacyPinyinAssembly -Force
}
Write-Output "Deployed ItemSpawnerEnhanced and bundled extensions to $deployDir"
