Add-Type -AssemblyName System.Drawing

$out = Join-Path (Get-Location) "jewel_clear_effect_sample.png"
$w = 1800
$h = 520
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(248, 248, 248))

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
    return [System.Drawing.Color]::FromArgb(
        [Math]::Max(0, [int]($c.R * (1 - $p))),
        [Math]::Max(0, [int]($c.G * (1 - $p))),
        [Math]::Max(0, [int]($c.B * (1 - $p)))
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
    $dx = ($mx - $cx) / 120.0
    $dy = ($my - $cy) / 120.0
    $score = (-0.45 * $dx) + (-0.25 * $dy) + (0.75 * $dx * $dy)
    if ($score -gt 0.52) { return 0.52 }
    if ($score -gt 0.25) { return 0.34 }
    if ($score -gt 0.05) { return 0.18 }
    if ($score -gt -0.20) { return -0.06 }
    if ($score -gt -0.42) { return -0.22 }
    return -0.36
}

function FillInnerGradient($g, [System.Drawing.PointF[]]$inner, [System.Drawing.Color]$base) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddPolygon($inner)
    $bounds = $path.GetBounds()
    $start = New-Object System.Drawing.PointF($bounds.Left, $bounds.Top)
    $end = New-Object System.Drawing.PointF($bounds.Right, $bounds.Bottom)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($start, $end, (Lighten $base 0.58), (Darken $base 0.08))
    $blend = New-Object System.Drawing.Drawing2D.ColorBlend
    $blend.Positions = [single[]]@(0.0, 0.42, 0.78, 1.0)
    $blend.Colors = [System.Drawing.Color[]]@((Lighten $base 0.62), (Lighten $base 0.14), $base, (Darken $base 0.10))
    $brush.InterpolationColors = $blend
    $g.FillPath($brush, $path)
    $brush.Dispose()
    $path.Dispose()
}

function DrawSharpHighlight($g, [double]$x, [double]$y, [double]$scale) {
    $white = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(238, 255, 255, 255))
    $poly = [System.Drawing.PointF[]]@(
        (Pt ($x) ($y - 26 * $scale)),
        (Pt ($x + 16 * $scale) ($y - 6 * $scale)),
        (Pt ($x + 48 * $scale) ($y)),
        (Pt ($x + 14 * $scale) ($y + 8 * $scale)),
        (Pt ($x - 2 * $scale) ($y + 30 * $scale)),
        (Pt ($x - 10 * $scale) ($y + 6 * $scale)),
        (Pt ($x - 38 * $scale) ($y)),
        (Pt ($x - 8 * $scale) ($y - 8 * $scale))
    )
    $g.FillPolygon($white, $poly)
    $white.Dispose()
}

function DrawRimLight($g, [System.Drawing.PointF[]]$inner) {
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(165, 255, 255, 255), 5)
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $n = $inner.Length
    for ($idx = [int]($n * 0.38); $idx -lt [int]($n * 0.64); $idx++) {
        $j = ($idx + 1) % $n
        $g.DrawLine($pen, $inner[$idx], $inner[$j])
    }
    $pen.Dispose()
}

function DrawClearGem($g, [System.Drawing.PointF[]]$outer, [System.Drawing.PointF[]]$inner, [System.Drawing.Color]$base) {
    $cx = 0.0
    $cy = 0.0
    foreach ($p in $outer) { $cx += $p.X; $cy += $p.Y }
    $cx = $cx / $outer.Length
    $cy = $cy / $outer.Length

    for ($idx = 0; $idx -lt $outer.Length; $idx++) {
        $j = ($idx + 1) % $outer.Length
        $shadeValue = ShadeForFacet $outer[$idx] $outer[$j] $cx $cy
        $shade = MixShade $base $shadeValue
        $brush = New-Object System.Drawing.SolidBrush $shade
        $facet = [System.Drawing.PointF[]]@($outer[$idx], $outer[$j], $inner[$j], $inner[$idx])
        $g.FillPolygon($brush, $facet)
        $brush.Dispose()
    }

    FillInnerGradient $g $inner $base
    DrawRimLight $g $inner
    DrawSharpHighlight $g ($cx - 42) ($cy - 44) 0.82

    $pen = New-Object System.Drawing.Pen (Darken $base 0.16), 2
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPolygon($pen, $outer)
    $pen.Dispose()
}

function BuildGem([string]$shape, [double]$x, [double]$y, [double]$slot) {
    if ($shape -eq "oct") {
        $s = $slot - 20
        $x += 10
        $y += 10
        $clip = 38
        $outer = [System.Drawing.PointF[]]@((Pt ($x+$clip) $y), (Pt ($x+$s-$clip) $y), (Pt ($x+$s) ($y+$clip)), (Pt ($x+$s) ($y+$s-$clip)), (Pt ($x+$s-$clip) ($y+$s)), (Pt ($x+$clip) ($y+$s)), (Pt $x ($y+$s-$clip)), (Pt $x ($y+$clip)))
        $inner = [System.Drawing.PointF[]]@((Pt ($x+58) ($y+34)), (Pt ($x+$s-58) ($y+34)), (Pt ($x+$s-34) ($y+58)), (Pt ($x+$s-34) ($y+$s-58)), (Pt ($x+$s-58) ($y+$s-34)), (Pt ($x+58) ($y+$s-34)), (Pt ($x+34) ($y+$s-58)), (Pt ($x+34) ($y+58)))
    }
    elseif ($shape -eq "square") {
        $s = $slot - 20
        $x += 10
        $y += 10
        $outer = [System.Drawing.PointF[]]@((Pt $x $y), (Pt ($x+$s) $y), (Pt ($x+$s) ($y+$s)), (Pt $x ($y+$s)))
        $inner = [System.Drawing.PointF[]]@((Pt ($x+44) ($y+44)), (Pt ($x+$s-44) ($y+44)), (Pt ($x+$s-44) ($y+$s-44)), (Pt ($x+44) ($y+$s-44)))
    }
    elseif ($shape -eq "diamond") {
        $s = $slot
        $cx = $x + $s / 2
        $cy = $y + $s / 2
        $outer = [System.Drawing.PointF[]]@((Pt $cx $y), (Pt ($x+$s*0.78) ($y+$s*0.15)), (Pt ($x+$s) $cy), (Pt ($x+$s*0.78) ($y+$s*0.85)), (Pt $cx ($y+$s)), (Pt ($x+$s*0.22) ($y+$s*0.85)), (Pt $x $cy), (Pt ($x+$s*0.22) ($y+$s*0.15)))
        $inner = [System.Drawing.PointF[]]@((Pt $cx ($y+$s*0.16)), (Pt ($x+$s*0.69) ($y+$s*0.31)), (Pt ($x+$s*0.84) $cy), (Pt ($x+$s*0.69) ($y+$s*0.69)), (Pt $cx ($y+$s*0.84)), (Pt ($x+$s*0.31) ($y+$s*0.69)), (Pt ($x+$s*0.16) $cy), (Pt ($x+$s*0.31) ($y+$s*0.31)))
    }
    elseif ($shape -eq "circle") {
        $outer = @()
        $inner = @()
        $cx = $x + $slot / 2
        $cy = $y + $slot / 2
        for ($k = 0; $k -lt 16; $k++) {
            $a = (-90 + $k * 22.5) * [Math]::PI / 180
            $outer += (Pt ($cx + [Math]::Cos($a)*($slot/2)) ($cy + [Math]::Sin($a)*($slot/2)))
            $inner += (Pt ($cx + [Math]::Cos($a)*($slot*0.34)) ($cy + [Math]::Sin($a)*($slot*0.34)))
        }
        $outer = [System.Drawing.PointF[]]$outer
        $inner = [System.Drawing.PointF[]]$inner
    }
    else {
        $s = $slot
        $outer = [System.Drawing.PointF[]]@((Pt ($x+$s/2) $y), (Pt ($x+$s) ($y+$s*0.38)), (Pt ($x+$s*0.81) ($y+$s)), (Pt ($x+$s*0.19) ($y+$s)), (Pt $x ($y+$s*0.38)))
        $inner = [System.Drawing.PointF[]]@((Pt ($x+$s/2) ($y+$s*0.15)), (Pt ($x+$s*0.82) ($y+$s*0.40)), (Pt ($x+$s*0.69) ($y+$s*0.82)), (Pt ($x+$s*0.31) ($y+$s*0.82)), (Pt ($x+$s*0.18) ($y+$s*0.40)))
    }
    return @{ outer=$outer; inner=$inner }
}

$xs = @(80, 410, 740, 1070, 1400)
$y = 140
$slot = 240
$shapes = @("oct", "square", "diamond", "circle", "pentagon")
$colors = @("#FF1E46", "#00D25A", "#0078FF", "#EB3CF5", "#FFDC00") | ForEach-Object { ColorFromHex $_ }

$fontTitle = New-Object System.Drawing.Font("Malgun Gothic", 24, [System.Drawing.FontStyle]::Bold)
$fontBody = New-Object System.Drawing.Font("Malgun Gothic", 14, [System.Drawing.FontStyle]::Regular)
$brushText = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(60, 60, 60))
$g.DrawString("맑은 보석 표현 추가 샘플", $fontTitle, $brushText, 70, 42)
$g.DrawString("내부 그라디언트 + 날카로운 흰색 하이라이트 + 하단 림라이트 적용", $fontBody, $brushText, 72, 86)

for ($idx = 0; $idx -lt 5; $idx++) {
    $gem = BuildGem $shapes[$idx] $xs[$idx] $y $slot
    DrawClearGem $g $gem.outer $gem.inner $colors[$idx]
}

$brushText.Dispose()
$fontTitle.Dispose()
$fontBody.Dispose()
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()
Write-Output $out
