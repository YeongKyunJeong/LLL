Add-Type -AssemblyName System.Drawing

$workspace = Get-Location
$samplePath = Join-Path $workspace "기획\디자인\UI\보석\보석_색상2종_상태2종_20종_샘플.png"
$out = Join-Path $workspace "보석_UI_컨셉_시트.png"

$w = 1900
$h = 2760
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$g.Clear([System.Drawing.Color]::FromArgb(248, 248, 248))

function Brush([int]$r, [int]$gValue, [int]$b) {
    return New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb($r, $gValue, $b))
}

function PenColor([int]$r, [int]$gValue, [int]$b, [float]$width = 1) {
    return New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb($r, $gValue, $b), $width)
}

function DrawText([string]$text, [System.Drawing.Font]$font, [System.Drawing.Brush]$brush, [float]$x, [float]$y, [float]$width, [float]$height) {
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Near
    $format.LineAlignment = [System.Drawing.StringAlignment]::Near
    $g.DrawString($text, $font, $brush, (New-Object System.Drawing.RectangleF($x, $y, $width, $height)), $format)
    $format.Dispose()
}

function DrawBox([float]$x, [float]$y, [float]$width, [float]$height, [string]$title, [string[]]$lines) {
    $fill = Brush 255 255 255
    $border = PenColor 220 220 220 1
    $rect = New-Object System.Drawing.RectangleF($x, $y, $width, $height)
    $g.FillRectangle($fill, $rect)
    $g.DrawRectangle($border, $x, $y, $width, $height)
    $fill.Dispose()
    $border.Dispose()

    DrawText $title $fontSub $brushDark ($x + 22) ($y + 18) ($width - 44) 40
    $body = [string]::Join("`n", $lines)
    DrawText $body $fontBody $brushBody ($x + 22) ($y + 68) ($width - 44) ($height - 88)
}

$fontTitle = New-Object System.Drawing.Font("Malgun Gothic", 34, [System.Drawing.FontStyle]::Bold)
$fontSub = New-Object System.Drawing.Font("Malgun Gothic", 22, [System.Drawing.FontStyle]::Bold)
$fontBody = New-Object System.Drawing.Font("Malgun Gothic", 17, [System.Drawing.FontStyle]::Regular)
$fontSmall = New-Object System.Drawing.Font("Malgun Gothic", 14, [System.Drawing.FontStyle]::Regular)
$fontWarn = New-Object System.Drawing.Font("Malgun Gothic", 18, [System.Drawing.FontStyle]::Bold)
$brushDark = Brush 35 35 35
$brushBody = Brush 65 65 65
$brushMuted = Brush 105 105 105
$brushAccent = Brush 160 60 20

DrawText "보석판 보석 UI 컨셉 시트" $fontTitle $brushDark 70 52 1000 60
DrawText "JewelBoard에 배치되는 보석 타일의 기본/선택 상태, 형태, 색상, 광원 규칙 정리" $fontBody $brushMuted 74 120 1200 36

DrawBox 70 190 850 300 "1. 용도와 공통 방향" @(
    "- 보석은 캐릭터 스킬 발동을 위한 입력 UI 요소입니다.",
    "- 중앙의 정면 평면은 스킬 아이콘 배치 영역이므로 단색에 가깝게 유지합니다.",
    "- 광원은 중앙 면이 아니라 외곽 테두리의 비스듬한 면 밝기 차이로 표현합니다.",
    "- 가장 밝은 면은 좌상단, 가장 어두운 면은 좌하단 기준입니다."
)

DrawBox 980 190 850 300 "2. 선택 상태 규칙" @(
    "- 선택/호버/연결 중인 보석은 기본 상태보다 한 단계 밝게 표현합니다.",
    "- 보석 뒤쪽에 후면 Glow를 추가해 선택 상태를 강조합니다.",
    "- 선택 상태에서는 기본색보다 더 진한 굵은 외곽선을 추가해 후광과 보석 형태를 분리합니다.",
    "- Pop 제거 연출은 별도 파티클로 처리하며, 이 시트에는 리소스 추가가 필요 없습니다."
)

DrawBox 70 530 850 330 "3. 형태 체계" @(
    "A 적색: 정사각형에서 모서리만 제거한 8각형",
    "B 녹색: 정사각형",
    "C 파랑: 각 변이 바깥으로 부푼 뚱뚱한 마름모",
    "D 보라: 원형",
    "E 노랑: 하단이 평면이고 상단 꼭짓점이 있는 오각형",
    "마름모, 원형, 오각형은 칸에 크게 배치하고 사각형 계열은 시각 균형을 위해 약간 작게 배치합니다."
)

DrawBox 980 530 850 330 "4. 색상 체계" @(
    "A: #FF1E46 / #FF6900",
    "B: #00D25A / #6EF014",
    "C: #0078FF / #00D2FF",
    "D: #EB3CF5 / #A028FF",
    "E: #FFDC00 / #FFF5BE",
    "각 클래스는 기본색 1종과 구분 가능한 변형색 1종을 사용합니다."
)

$warnFill = Brush 255 248 220
$warnBorder = PenColor 230 195 90 2
$g.FillRectangle($warnFill, 70, 900, 1760, 92)
$g.DrawRectangle($warnBorder, 70, 900, 1760, 92)
DrawText "중요: 아래 샘플의 연한 정사각형 틀은 보석 크기 비교용 가이드입니다. 실제 디자인 리소스에는 포함되지 않습니다." $fontWarn $brushAccent 100 928 1700 42
$warnFill.Dispose()
$warnBorder.Dispose()

$noteFill = Brush 238 246 255
$noteBorder = PenColor 170 200 235 2
$g.FillRectangle($noteFill, 70, 1008, 1760, 70)
$g.DrawRectangle($noteBorder, 70, 1008, 1760, 70)
DrawText "참고: 본 보석 이미지는 컨셉 확인용 샘플이며, 추후 디자인 확정 과정에서 변경될 수 있습니다." $fontWarn $brushMuted 100 1028 1700 34
$noteFill.Dispose()
$noteBorder.Dispose()

if (Test-Path -LiteralPath $samplePath) {
    $sample = [System.Drawing.Image]::FromFile($samplePath)
    $targetW = 1760
    $ratio = $targetW / $sample.Width
    $targetH = [int]($sample.Height * $ratio)
    $g.DrawImage($sample, 70, 1120, $targetW, $targetH)
    $sample.Dispose()
}
else {
    DrawText "샘플 이미지 파일을 찾을 수 없습니다." $fontBody $brushAccent 70 1120 1000 40
}

DrawText "샘플 구성: 5가지 형태/색상 클래스 x 클래스별 세부 색 2종 x 기본/선택 상태 2종 = 총 20종" $fontSmall $brushMuted 70 2680 1500 36

$brushDark.Dispose()
$brushBody.Dispose()
$brushMuted.Dispose()
$brushAccent.Dispose()
$fontTitle.Dispose()
$fontSub.Dispose()
$fontBody.Dispose()
$fontSmall.Dispose()
$fontWarn.Dispose()

$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()
Write-Output $out
