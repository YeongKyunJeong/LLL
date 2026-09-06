Add-Type -AssemblyName System.Drawing

$out = Join-Path (Get-Location) "jewel_20_state_sample.png"
$w = 1700
$h = 1320
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$g.Clear([System.Drawing.Color]::FromArgb(247, 247, 247))

function ColorFromHex([string]$hex) {
    $hex = $hex.TrimStart('#')
    return [System.Drawing.Color]::FromArgb(
        [Convert]::ToInt32($hex.Substring(0, 2), 16),
        [Convert]::ToInt32($hex.Substring(2, 2), 16),
        [Convert]::ToInt32($hex.Substring(4, 2), 16)
    )
}

function Lighten([System.Drawing.Color]$c, [double]$p) {
    return [System.Drawing.Color]::FromArgb(
        [Math]::Min(255, [int]($c.R + (255 - $c.R) * $p)),
        [Math]::Min(255, [int]($c.G + (255 - $c.G) * $p)),
        [Math]::Min(255, [int]($c.B + (255 - $c.B) * $p))
    )
}

function Darken([System.Drawing.Color]$c, [double]$p) {
    $max = [Math]::Max($c.R, [Math]::Max($c.G, $c.B))
    $majorFactor = 1 - ($p * 0.92)
    $minorFactor = 1 - ($p * 1.30)
    if ($majorFactor -lt 0) { $majorFactor = 0 }
    if ($minorFactor -lt 0) { $minorFactor = 0 }

    $rFactor = if ($c.R -ge ($max - 8)) { $majorFactor } else { $minorFactor }
    $gFactor = if ($c.G -ge ($max - 8)) { $majorFactor } else { $minorFactor }
    $bFactor = if ($c.B -ge ($max - 8)) { $majorFactor } else { $minorFactor }

    return [System.Drawing.Color]::FromArgb(
        [Math]::Max(0, [int]($c.R * $rFactor)),
        [Math]::Max(0, [int]($c.G * $gFactor)),
        [Math]::Max(0, [int]($c.B * $bFactor))
    )
}

function Pt([double]$x, [double]$y) {
    return New-Object System.Drawing.PointF([single]$x, [single]$y)
}

function MixShade([System.Drawing.Color]$base, [double]$v) {
    if ($v -ge 0) { return Lighten $base $v }
    return Darken $base (-$v)
}

function ShadeForFacet([System.Drawing.PointF]$a, [System.Drawing.PointF]$b, [double]$cx, [double]$cy) {
    $mx = ($a.X + $b.X) / 2.0
    $my = ($a.Y + $b.Y) / 2.0
    $dx = ($mx - $cx) / 96.0
    $dy = ($my - $cy) / 96.0
    # Main light from upper-left, weak fill light from lower-right.
    $distUL = [Math]::Sqrt([Math]::Pow($dx + 1.0, 2) + [Math]::Pow($dy + 1.0, 2))
    $distBR = [Math]::Sqrt([Math]::Pow($dx - 1.0, 2) + [Math]::Pow($dy - 1.0, 2))
    $main = [Math]::Max(0.0, 1.0 - ($distUL / 2.4))
    $fill = [Math]::Max(0.0, 1.0 - ($distBR / 2.4))
    $falloff = [Math]::Min(1.0, $distUL / 1.9)
    $score = (0.70 * $main) + (0.18 * $fill) - (0.42 * $falloff)

    if ($dx -gt 0.42 -and $dy -gt 0.42) {
        $score += 0.08
    }

    if ($score -gt 0.42) { return 0.54 }
    if ($score -gt 0.24) { return 0.26 }
    if ($score -gt 0.08) { return 0.06 }
    if ($score -gt -0.06) { return -0.16 }
    if ($score -gt -0.18) { return -0.30 }
    return -0.46
}

function DrawGlow($g, [System.Drawing.PointF[]]$outer, [System.Drawing.Color]$base) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddPolygon($outer)
    for ($k = 6; $k -ge 1; $k--) {
        $alpha = [int](18 + $k * 10)
        $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb($alpha, (Lighten $base 0.45)), ($k * 5))
        $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
        $g.DrawPath($pen, $path)
        $pen.Dispose()
    }
    $path.Dispose()
}

function FillInnerClearFace($g, [System.Drawing.PointF[]]$inner, [System.Drawing.Color]$base) {
    $brush = New-Object System.Drawing.SolidBrush $base
    $g.FillPolygon($brush, $inner)
    $brush.Dispose()
}

function DrawPolygonGem($g, [System.Drawing.PointF[]]$outer, [System.Drawing.PointF[]]$inner, [System.Drawing.Color]$base, [bool]$selected) {
    $drawBase = if ($selected) { Lighten $base 0.16 } else { $base }
    if ($selected) { DrawGlow $g $outer $drawBase }

    $cx = 0.0
    $cy = 0.0
    foreach ($p in $outer) {
        $cx += $p.X
        $cy += $p.Y
    }
    $cx = $cx / $outer.Length
    $cy = $cy / $outer.Length

    for ($idx = 0; $idx -lt $outer.Length; $idx++) {
        $j = ($idx + 1) % $outer.Length
        $shadeValue = ShadeForFacet $outer[$idx] $outer[$j] $cx $cy
        if ($selected) { $shadeValue += 0.08 }
        $shade = MixShade $drawBase $shadeValue
        $brush = New-Object System.Drawing.SolidBrush $shade
        $facet = [System.Drawing.PointF[]]@($outer[$idx], $outer[$j], $inner[$j], $inner[$idx])
        $g.FillPolygon($brush, $facet)
        $brush.Dispose()
    }

    FillInnerClearFace $g $inner $drawBase

    $outline = if ($selected) { Darken $base 0.38 } else { Darken $drawBase 0.18 }
    $width = if ($selected) { 6 } else { 2 }
    $pen = New-Object System.Drawing.Pen $outline, $width
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPolygon($pen, $outer)
    $pen.Dispose()
}

function BuildGem([string]$shape, [double]$x, [double]$y, [double]$slot) {
    if ($shape -eq "oct") {
        $s = $slot - 16
        $x += 8
        $y += 8
        $clip = 34
        $outer = [System.Drawing.PointF[]]@(
            (Pt ($x + $clip) $y), (Pt ($x + $s - $clip) $y), (Pt ($x + $s) ($y + $clip)), (Pt ($x + $s) ($y + $s - $clip)),
            (Pt ($x + $s - $clip) ($y + $s)), (Pt ($x + $clip) ($y + $s)), (Pt $x ($y + $s - $clip)), (Pt $x ($y + $clip))
        )
        $inner = [System.Drawing.PointF[]]@(
            (Pt ($x + 42) ($y + 24)), (Pt ($x + $s - 42) ($y + 24)), (Pt ($x + $s - 24) ($y + 42)), (Pt ($x + $s - 24) ($y + $s - 42)),
            (Pt ($x + $s - 42) ($y + $s - 24)), (Pt ($x + 42) ($y + $s - 24)), (Pt ($x + 24) ($y + $s - 42)), (Pt ($x + 24) ($y + 42))
        )
    }
    elseif ($shape -eq "square") {
        $s = $slot - 16
        $x += 8
        $y += 8
        $outer = [System.Drawing.PointF[]]@((Pt $x $y), (Pt ($x + $s) $y), (Pt ($x + $s) ($y + $s)), (Pt $x ($y + $s)))
        $inner = [System.Drawing.PointF[]]@((Pt ($x + 24) ($y + 24)), (Pt ($x + $s - 24) ($y + 24)), (Pt ($x + $s - 24) ($y + $s - 24)), (Pt ($x + 24) ($y + $s - 24)))
    }
    elseif ($shape -eq "diamond") {
        $s = $slot
        $cx = $x + $s / 2
        $cy = $y + $s / 2
        $outer = [System.Drawing.PointF[]]@(
            (Pt $cx $y), (Pt ($x + $s * 0.78) ($y + $s * 0.15)), (Pt ($x + $s) $cy), (Pt ($x + $s * 0.78) ($y + $s * 0.85)),
            (Pt $cx ($y + $s)), (Pt ($x + $s * 0.22) ($y + $s * 0.85)), (Pt $x $cy), (Pt ($x + $s * 0.22) ($y + $s * 0.15))
        )
        $inner = [System.Drawing.PointF[]]@(
            (Pt $cx ($y + $s * 0.16)), (Pt ($x + $s * 0.69) ($y + $s * 0.31)), (Pt ($x + $s * 0.84) $cy), (Pt ($x + $s * 0.69) ($y + $s * 0.69)),
            (Pt $cx ($y + $s * 0.84)), (Pt ($x + $s * 0.31) ($y + $s * 0.69)), (Pt ($x + $s * 0.16) $cy), (Pt ($x + $s * 0.31) ($y + $s * 0.31))
        )
    }
    elseif ($shape -eq "circle") {
        $outer = @()
        $inner = @()
        $cx = $x + $slot / 2
        $cy = $y + $slot / 2
        for ($k = 0; $k -lt 16; $k++) {
            $a = (-90 + $k * 22.5) * [Math]::PI / 180
            $outer += (Pt ($cx + [Math]::Cos($a) * ($slot / 2)) ($cy + [Math]::Sin($a) * ($slot / 2)))
            $inner += (Pt ($cx + [Math]::Cos($a) * ($slot * 0.34)) ($cy + [Math]::Sin($a) * ($slot * 0.34)))
        }
        $outer = [System.Drawing.PointF[]]$outer
        $inner = [System.Drawing.PointF[]]$inner
    }
    else {
        $s = $slot
        $outer = [System.Drawing.PointF[]]@(
            (Pt ($x + $s / 2) $y), (Pt ($x + $s) ($y + $s * 0.38)), (Pt ($x + $s * 0.81) ($y + $s)),
            (Pt ($x + $s * 0.19) ($y + $s)), (Pt $x ($y + $s * 0.38))
        )
        $inner = [System.Drawing.PointF[]]@(
            (Pt ($x + $s / 2) ($y + $s * 0.15)), (Pt ($x + $s * 0.82) ($y + $s * 0.40)), (Pt ($x + $s * 0.69) ($y + $s * 0.82)),
            (Pt ($x + $s * 0.31) ($y + $s * 0.82)), (Pt ($x + $s * 0.18) ($y + $s * 0.40))
        )
    }
    return @{ outer = $outer; inner = $inner }
}

$fontTitle = New-Object System.Drawing.Font("Arial", 20, [System.Drawing.FontStyle]::Bold)
$fontSmall = New-Object System.Drawing.Font("Arial", 14, [System.Drawing.FontStyle]::Regular)
$brushText = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(70, 70, 70))
$g.DrawString("Jewel samples: 5 colors x 2 color variants x Base/Selected", $fontTitle, $brushText, 60, 28)
$headers = @("Color 1 Base", "Color 1 Selected", "Color 2 Base", "Color 2 Selected")
$colX = @(360, 660, 960, 1260)
for ($i = 0; $i -lt 4; $i++) {
    $g.DrawString($headers[$i], $fontSmall, $brushText, $colX[$i] + 18, 86)
}

$rows = @(
    @{ label = "A Red Oct"; shape = "oct"; colors = @("#FF1E46", "#FF6900") },
    @{ label = "B Green Square"; shape = "square"; colors = @("#00D25A", "#6EF014") },
    @{ label = "C Blue Diamond"; shape = "diamond"; colors = @("#0078FF", "#00D2FF") },
    @{ label = "D Purple Circle"; shape = "circle"; colors = @("#EB3CF5", "#A028FF") },
    @{ label = "E Yellow Pentagon"; shape = "pentagon"; colors = @("#FFDC00", "#FFF5BE") }
)
$rowY = @(150, 380, 610, 840, 1070)
$slot = 160

for ($r = 0; $r -lt $rows.Count; $r++) {
    $g.DrawString($rows[$r].label, $fontSmall, $brushText, 62, $rowY[$r] + 64)
    for ($variant = 0; $variant -lt 2; $variant++) {
        for ($state = 0; $state -lt 2; $state++) {
            $col = $variant * 2 + $state
            $x = $colX[$col]
            $y = $rowY[$r]
            $slotPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(222, 222, 222), 1)
            $g.DrawRectangle($slotPen, [single]$x, [single]$y, $slot, $slot)
            $slotPen.Dispose()
            $gem = BuildGem $rows[$r].shape $x $y $slot
            $base = ColorFromHex $rows[$r].colors[$variant]
            $selected = ($state -eq 1)
            DrawPolygonGem $g $gem.outer $gem.inner $base $selected
        }
    }
}

$brushText.Dispose()
$fontTitle.Dispose()
$fontSmall.Dispose()
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()
Write-Output $out
