param(
    [Parameter(Mandatory = $true)]
    [string] $Source,
    [Parameter(Mandatory = $true)]
    [string] $ProductionOutput,
    [Parameter(Mandatory = $true)]
    [string] $PreviewOutput
)

Add-Type -AssemblyName System.Drawing

$sourceImage = [System.Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $Source))
try {
    $left = $sourceImage.Width
    $top = $sourceImage.Height
    $right = -1
    $bottom = -1

    for ($y = 0; $y -lt $sourceImage.Height; $y++) {
        for ($x = 0; $x -lt $sourceImage.Width; $x++) {
            if ($sourceImage.GetPixel($x, $y).A -gt 8) {
                $left = [Math]::Min($left, $x)
                $top = [Math]::Min($top, $y)
                $right = [Math]::Max($right, $x)
                $bottom = [Math]::Max($bottom, $y)
            }
        }
    }

    if ($right -lt $left -or $bottom -lt $top) {
        throw 'The source image contains no visible pixels.'
    }

    $crop = [System.Drawing.Rectangle]::FromLTRB($left, $top, $right + 1, $bottom + 1)

    function Export-ContainedPng([int] $width, [int] $height, [int] $padding, [string] $path) {
        $canvas = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($canvas)
            try {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

                $scale = [Math]::Min(($width - 2 * $padding) / $crop.Width, ($height - 2 * $padding) / $crop.Height)
                $drawWidth = [int][Math]::Round($crop.Width * $scale)
                $drawHeight = [int][Math]::Round($crop.Height * $scale)
                $drawX = [int][Math]::Round(($width - $drawWidth) / 2)
                $drawY = [int][Math]::Round(($height - $drawHeight) / 2)
                $destination = New-Object System.Drawing.Rectangle($drawX, $drawY, $drawWidth, $drawHeight)
                $graphics.DrawImage($sourceImage, $destination, $crop, [System.Drawing.GraphicsUnit]::Pixel)
            }
            finally {
                $graphics.Dispose()
            }

            $outputDirectory = Split-Path -Parent $path
            if ($outputDirectory) {
                New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
            }
            $canvas.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $canvas.Dispose()
        }
    }

    Export-ContainedPng 256 224 8 $ProductionOutput
    Export-ContainedPng 512 448 16 $PreviewOutput
    Write-Output "source=$($sourceImage.Width)x$($sourceImage.Height) crop=$($crop.Width)x$($crop.Height) production=256x224 preview=512x448"
}
finally {
    $sourceImage.Dispose()
}
