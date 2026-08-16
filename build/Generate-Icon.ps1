param([string]$OutputPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'icon.png'))

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$bitmap = [System.Drawing.Bitmap]::new(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

try {
    $graphics.Clear([System.Drawing.Color]::FromArgb(25, 30, 31))

    $mountain = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $mountain.AddPolygon([System.Drawing.Point[]]@(
        [System.Drawing.Point]::new(18, 188),
        [System.Drawing.Point]::new(92, 76),
        [System.Drawing.Point]::new(128, 125),
        [System.Drawing.Point]::new(163, 69),
        [System.Drawing.Point]::new(238, 188)
    ))
    $mountainBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(62, 78, 78))
    $graphics.FillPath($mountainBrush, $mountain)

    $crateBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(75, 199, 148))
    $cratePen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(229, 239, 232), 7)
    $graphics.FillRectangle($crateBrush, 68, 96, 120, 104)
    $graphics.DrawRectangle($cratePen, 68, 96, 120, 104)
    $graphics.DrawLine($cratePen, 70, 129, 186, 129)
    $graphics.DrawLine($cratePen, 91, 97, 91, 199)
    $graphics.DrawLine($cratePen, 165, 97, 165, 199)

    $plusPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(244, 177, 92), 13)
    $plusPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $plusPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLine($plusPen, 128, 143, 128, 181)
    $graphics.DrawLine($plusPen, 109, 162, 147, 162)
}
finally {
    if ($plusPen) { $plusPen.Dispose() }
    if ($cratePen) { $cratePen.Dispose() }
    if ($crateBrush) { $crateBrush.Dispose() }
    if ($mountainBrush) { $mountainBrush.Dispose() }
    if ($mountain) { $mountain.Dispose() }
    $graphics.Dispose()
}

$resolved = [System.IO.Path]::GetFullPath($OutputPath)
$parent = Split-Path -Parent $resolved
if (-not (Test-Path -LiteralPath $parent)) {
    New-Item -ItemType Directory -Path $parent | Out-Null
}
$bitmap.Save($resolved, [System.Drawing.Imaging.ImageFormat]::Png)
$bitmap.Dispose()
Write-Output "Created $resolved"

