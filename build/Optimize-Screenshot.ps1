param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [ValidateRange(1, 100)]
    [int]$Quality = 80,

    [ValidateRange(1, 16383)]
    [int]$Width = 1600,

    [ValidateRange(1, 16383)]
    [int]$Height = 1000,

    [string]$CwebpPath
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$toolVersion = '1.5.0'
$toolArchiveName = "libwebp-$toolVersion-windows-x64.zip"
$toolArchiveHash = 'E8FE3BC7EB09774E69261A42BF9FA8A37AB5F3EECAAB199F6420E6F9E822090C'
$toolUri = "https://storage.googleapis.com/downloads.webmproject.org/releases/webp/$toolArchiveName"
$toolDirectory = Join-Path $projectRoot 'artifacts\tools'
$downloadedEncoder = Join-Path $toolDirectory "libwebp-$toolVersion-windows-x64\bin\cwebp.exe"

$resolvedInput = [System.IO.Path]::GetFullPath($InputPath)
if (-not (Test-Path -LiteralPath $resolvedInput -PathType Leaf)) {
    throw "Input image was not found: $resolvedInput"
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
if ([System.IO.Path]::GetExtension($resolvedOutput) -ine '.webp') {
    throw "OutputPath must use the .webp extension: $resolvedOutput"
}

Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile($resolvedInput)
try {
    $inputRatio = $image.Width / $image.Height
    $outputRatio = $Width / $Height
    if ([Math]::Abs($inputRatio - $outputRatio) -gt 0.001) {
        throw "Input aspect ratio $($image.Width)x$($image.Height) does not match target $($Width)x$($Height). Crop the source image first."
    }
}
finally {
    $image.Dispose()
}

if (-not [string]::IsNullOrWhiteSpace($CwebpPath)) {
    $encoder = [System.IO.Path]::GetFullPath($CwebpPath)
    if (-not (Test-Path -LiteralPath $encoder -PathType Leaf)) {
        throw "cwebp was not found: $encoder"
    }
}
else {
    $installedEncoder = Get-Command cwebp -CommandType Application -ErrorAction SilentlyContinue
    if ($installedEncoder) {
        $encoder = $installedEncoder.Source
    }
    else {
        $encoder = $downloadedEncoder
        if (-not (Test-Path -LiteralPath $encoder -PathType Leaf)) {
            New-Item -ItemType Directory -Path $toolDirectory -Force | Out-Null
            $archive = Join-Path $toolDirectory $toolArchiveName
            if (-not (Test-Path -LiteralPath $archive -PathType Leaf)) {
                Write-Output "Downloading libwebp $toolVersion..."
                Invoke-WebRequest -UseBasicParsing -Uri $toolUri -OutFile $archive
            }

            $actualHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
            if ($actualHash -cne $toolArchiveHash) {
                throw "libwebp archive hash mismatch. Expected $toolArchiveHash, got $actualHash."
            }

            Expand-Archive -LiteralPath $archive -DestinationPath $toolDirectory -Force
            if (-not (Test-Path -LiteralPath $encoder -PathType Leaf)) {
                throw "cwebp was not found after extracting $archive"
            }
        }
    }
}

$outputDirectory = Split-Path -Parent $resolvedOutput
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

& $encoder `
    -quiet `
    -q $Quality `
    -m 6 `
    -mt `
    -sharp_yuv `
    -metadata none `
    -resize $Width $Height `
    $resolvedInput `
    -o $resolvedOutput
if ($LASTEXITCODE -ne 0) {
    throw "cwebp failed with exit code $LASTEXITCODE"
}

$output = Get-Item -LiteralPath $resolvedOutput
Write-Output "Created $($output.FullName) ($($output.Length) bytes, ${Width}x$Height, quality $Quality)"
