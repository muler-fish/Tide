# 使用系统绘图库绘制图标，不需要下载素材；输出覆盖项目自身的 Assets/tide.ico。
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assetRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\src\Tide.Desktop\Assets'))
[IO.Directory]::CreateDirectory($assetRoot) | Out-Null
$images = [Collections.Generic.List[byte[]]]::new()
# 同一套 256 坐标系缩放生成三个尺寸，分别用于小图标、常规图标和高分辨率显示。
foreach ($size in 32,48,256) {
    $bitmap = [Drawing.Bitmap]::new($size,$size)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.ScaleTransform($size/256.0,$size/256.0)
    $path = [Drawing.Drawing2D.GraphicsPath]::new()
    $path.AddArc(8,8,88,88,180,90)
    $path.AddArc(160,8,88,88,270,90)
    $path.AddArc(160,160,88,88,0,90)
    $path.AddArc(8,160,88,88,90,90)
    $path.CloseFigure()
    $brush = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#13765E'))
    $graphics.FillPath($brush,$path)
    $pen = [Drawing.Pen]::new([Drawing.Color]::White,14)
    $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    # 两条贝塞尔曲线组成海浪，与主窗口的波浪标记保持相同的视觉方向。
    foreach ($offset in 0,56) {
        $wave = [Drawing.Drawing2D.GraphicsPath]::new()
        $wave.AddBezier(48,(104+$offset),80,(52+$offset),112,(156+$offset),144,(104+$offset))
        $wave.AddBezier(144,(104+$offset),166,(70+$offset),188,(72+$offset),208,(92+$offset))
        $graphics.DrawPath($pen,$wave)
        $wave.Dispose()
    }
    $stream = [IO.MemoryStream]::new()
    $bitmap.Save($stream,[Drawing.Imaging.ImageFormat]::Png)
    $images.Add($stream.ToArray())
    $stream.Dispose(); $pen.Dispose(); $brush.Dispose(); $path.Dispose(); $graphics.Dispose(); $bitmap.Dispose()
}
$iconPath = Join-Path $assetRoot 'tide.ico'
$file = [IO.File]::Create($iconPath)
$writer = [IO.BinaryWriter]::new($file)
# ICO 文件由 6 字节文件头、每项 16 字节的索引表和各尺寸的 PNG 数据组成。
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]3)
$offsetBytes = 6+16*3
# ICO 索引里的宽高只有一个字节，数值 0 约定表示 256 像素。
$sizes = @(32,48,0)
for ($i=0; $i -lt 3; $i++) {
    $writer.Write([byte]$sizes[$i]); $writer.Write([byte]$sizes[$i]); $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([uint16]1); $writer.Write([uint16]32)
    $writer.Write([uint32]$images[$i].Length); $writer.Write([uint32]$offsetBytes)
    $offsetBytes += $images[$i].Length
}
foreach ($bytes in $images) { $writer.Write($bytes) }
$writer.Dispose(); $file.Dispose()
Get-Item -LiteralPath $iconPath | Select-Object FullName,Length
