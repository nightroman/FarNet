
<#
.SYNOPSIS
	Shows image(s) in a GUI window.

.DESCRIPTION
	This is a simple way to take a look at a picture or several pictures. It is
	fast and it does not need any extra software. It is convenient to use from
	Far Manager via file associations or user menu commands, see examples.

	The script can be called directly but in this case its modal dialog blocks
	the calling thread. To avoid this this script should be called as a job or
	by starting a separate PowerShell process in a hidden window.

	For a single image the window is resizable. For several images it is not,
	images are simply placed from left to right and scaled, if needed.

.INPUTS
	Image file paths are passed in as arguments or piped.

.EXAMPLE
	# Far Manager association: internal way: faster but picture windows will be
	# closed together with the Far window on exit.
	>: Start-FarJob -Hidden Show-Image (Get-FarPath) #

.EXAMPLE
	# Far Manager association: external way: slower but picture windows will be
	# opened even after closing the Far window.
	start /min powershell -WindowStyle Hidden -File C:\Scripts\Show-Image.ps1 "!\!.!"

.EXAMPLE
	# Far Manager user menu: internal way: show (several) selected images
	>: Start-FarJob -Hidden Show-Image (Get-FarPath -Selected) #
#>

Add-Type -AssemblyName System.Windows.Forms

### process input, create bitmaps
$files = if ($args) { $args } else { $input }
$count = 0
$width = 0
$height = 0
$images = foreach($file in $files) {
	try {
		$image = New-Object System.Drawing.Bitmap $file
		$width += $image.Size.Width
		if ($height -lt $image.Size.Height) {
			$height = $image.Size.Height
		}
		$image
		++$count
	}
	catch {}
}
if (!$images) {
	return
}

### create a form
$form = New-Object System.Windows.Forms.Form
$form.StartPosition = 'CenterScreen'
$form.Text = ($files | Split-Path -Leaf) -join ', '
if ($count -eq 1) {
	$form.FormBorderStyle = 'Sizable'
	$form.MaximizeBox = $true
}
else {
	$form.FormBorderStyle = 'FixedDialog'
	$form.MaximizeBox = $false
}
$form.add_KeyDown({ switch($_.KeyCode) { 'Escape' { $this.Close() } } })
$form.add_Shown({ $form.Activate() })

### get scale factor and set form size
$scale = 1
$maxsize = [System.Windows.Forms.SystemInformation]::MaxWindowTrackSize
if ($width -gt $maxsize.Width) {
	$scale = $maxsize.Width / $width * 0.95
	$width = $width * $scale
	$height = $height * $scale
}
if ($height -gt $maxsize.Height) {
	$scale = $maxsize.Height / $height * 0.95
	$width = $width * $scale
	$height = $height * $scale
}
$form.ClientSize = New-Object System.Drawing.Size $width, $height

### add picture boxes
$left = 0
foreach($image in $images) {
	$box = New-Object System.Windows.Forms.PictureBox
	$box.Image = $image
	$box.SizeMode = 'Zoom'
	$box.Size = New-Object System.Drawing.Size ($image.Size.Width * $scale), ($image.Size.Height * $scale)
	$box.Left = $left
	$left = $box.Right
	if ($count -eq 1) {
		$box.Dock = 'Fill'
	}
	else {
		$box.BorderStyle = 'FixedSingle'
	}
	$form.Controls.Add($box)
}

# show!
[void]$form.ShowDialog()

# clean
foreach($image in $images) {
	$image.Dispose()
}
