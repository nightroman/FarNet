# https://github.com/PowerShell/PowerShell/discussions/24562

#! needed in Desktop
if ($PSEdition -eq 'Desktop') {
	Add-Type -AssemblyName System.Windows.Forms
}

$path = "$PSScriptRoot\..\..\Zoo\FarNetLogo.png"
$bm = [System.Drawing.Bitmap]::new($path)
$size = $bm.Size
if ($size.Width -ne 60) {throw}
if ($size.Height -ne 55) {throw}
