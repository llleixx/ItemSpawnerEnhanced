param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$coreProject = Join-Path $projectRoot 'src\ItemSpawnerEnhanced\ItemSpawnerEnhanced.csproj'
$chineseProject = Join-Path $projectRoot 'src\ItemSpawnerEnhanced.ChineseSearch\ItemSpawnerEnhanced.ChineseSearch.csproj'
$coreOutputDir = Join-Path $projectRoot "src\ItemSpawnerEnhanced\bin\$Configuration"
$chineseOutputDir = Join-Path $projectRoot "src\ItemSpawnerEnhanced.ChineseSearch\bin\$Configuration"
$artifactDir = Join-Path $projectRoot 'artifacts'
$manifestPath = Join-Path $projectRoot 'manifest.json'

$propertiesOutput = (& dotnet msbuild $coreProject -nologo `
    -getProperty:Version `
    -getProperty:ThunderstoreAuthor `
    -getProperty:ThunderstorePackageName) -join [Environment]::NewLine
if ($LASTEXITCODE -ne 0) { throw 'Could not read package properties.' }
$jsonStart = $propertiesOutput.IndexOf('{')
if ($jsonStart -lt 0) { throw 'MSBuild did not return package properties as JSON.' }
$properties = ($propertiesOutput.Substring($jsonStart) | ConvertFrom-Json).Properties
$version = $properties.Version
$author = $properties.ThunderstoreAuthor
$packageName = $properties.ThunderstorePackageName

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.name -cne $packageName -or $manifest.version_number -cne $version) {
    throw 'manifest.json package name/version does not match the project.'
}
if ($manifest.dependencies -contains 'Hamunii-AutoHookGenPatcher-1.0.7') {
    throw 'AutoHookGenPatcher must not be packaged as a dependency.'
}

if (-not $SkipBuild) {
    & dotnet build $chineseProject -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) { throw 'ItemSpawnerEnhanced build failed.' }
}

$coreAssembly = Join-Path $coreOutputDir 'ItemSpawnerEnhanced.dll'
$chineseAssembly = Join-Path $chineseOutputDir 'ItemSpawnerEnhanced.ChineseSearch.dll'
foreach ($required in @($coreAssembly, $chineseAssembly)) {
    if (-not (Test-Path -LiteralPath $required)) { throw "Required output was not found: $required" }
}

$expectedAssemblyVersion = [Version]::Parse("$version.0")
foreach ($ownedAssembly in @($coreAssembly, $chineseAssembly)) {
    $actualAssemblyVersion = [System.Reflection.AssemblyName]::GetAssemblyName($ownedAssembly).Version
    if ($actualAssemblyVersion -ne $expectedAssemblyVersion) {
        throw "DLL assembly version '$actualAssemblyVersion' does not match '$expectedAssemblyVersion': $ownedAssembly"
    }
}

$icon = Join-Path $projectRoot 'icon.png'
if (-not (Test-Path -LiteralPath $icon)) {
    & (Join-Path $PSScriptRoot 'Generate-Icon.ps1') -OutputPath $icon
}
Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile($icon)
try {
    if ($image.Width -ne 256 -or $image.Height -ne 256) {
        throw "icon.png must be 256x256, got $($image.Width)x$($image.Height)."
    }
}
finally { $image.Dispose() }

$packageId = "$author-$packageName"
$stageDir = Join-Path $artifactDir "staging\$packageId"
$pluginDir = Join-Path $stageDir 'plugins\ItemSpawnerEnhanced'
$archive = Join-Path $artifactDir "$packageId-$version.zip"

$resolvedStage = [System.IO.Path]::GetFullPath($stageDir)
$resolvedArtifacts = [System.IO.Path]::GetFullPath($artifactDir)
if (-not $resolvedStage.StartsWith($resolvedArtifacts + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clean staging path outside artifacts: $resolvedStage"
}
if (Test-Path -LiteralPath $stageDir) { Remove-Item -LiteralPath $stageDir -Recurse -Force }
New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null

foreach ($file in @('manifest.json', 'icon.png', 'README.md', 'CHANGELOG.md', 'LICENSE', 'THIRD_PARTY_NOTICES.md')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $file) -Destination $stageDir
}
Copy-Item -LiteralPath $coreAssembly, $chineseAssembly -Destination $pluginDir

if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive -Force }
Compress-Archive -Path (Join-Path $stageDir '*') -DestinationPath $archive -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($archive)
try {
    $actual = @($zip.Entries | Where-Object { -not $_.FullName.EndsWith('/') } |
        ForEach-Object { $_.FullName.Replace('\', '/') } | Sort-Object)
    $expected = @(
        'plugins/ItemSpawnerEnhanced/ItemSpawnerEnhanced.dll',
        'plugins/ItemSpawnerEnhanced/ItemSpawnerEnhanced.ChineseSearch.dll',
        'CHANGELOG.md', 'LICENSE', 'README.md', 'THIRD_PARTY_NOTICES.md',
        'icon.png', 'manifest.json'
    ) | Sort-Object
    if (Compare-Object $expected $actual) {
        throw "Unexpected archive entries: $($actual -join ', ')"
    }
}
finally { $zip.Dispose() }

Remove-Item -LiteralPath $stageDir -Recurse -Force
Write-Output "Created $archive"
