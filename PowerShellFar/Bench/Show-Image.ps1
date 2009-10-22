
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

	For a single image the window is resizable. More than one input images are
	placed from left to right and scaled, if needed; click on a picture opens a
	separate modal window with this picture.

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

### input, create bitmaps
$files = if ($args) { $args } else { $input }
if (!$files) { $files = Get-Item *.bmp, *.gif, *.jpg, *.jpeg, *.png }
$width = 0
$height = 0
$images = New-Object System.Collections.ArrayList
foreach($file in $files) {
	try {
		if ($file -is [System.Collections.IDictionary]) {
			$image = $file
		}
		else {
			$path = $file.ToString()
			$image = @{
				Bitmap = New-Object System.Drawing.Bitmap $path
				Name = Split-Path $path -Leaf
			}
		}
		$bitmap = $image.Bitmap
		$width += $bitmap.Size.Width
		if ($height -lt $bitmap.Size.Height) {
			$height = $bitmap.Size.Height
		}
		$null = $images.Add($image)
	}
	catch {}
}
if ($images.Count -eq 0) {
	return
}

### create a form
$form = New-Object System.Windows.Forms.Form
$form.Text = ($images | .{process{ $_.Name }}) -join ', '
$form.StartPosition = 'CenterScreen'
if ($images.Count -eq 1) {
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
	$scale = $maxsize.Width / $width * 0.98
	$width = $width * $scale
	$height = $height * $scale
}
if ($height -gt $maxsize.Height) {
	$scale = $maxsize.Height / $height * 0.98
	$width = $width * $scale
	$height = $height * $scale
}
$form.ClientSize = New-Object System.Drawing.Size $width, $height

### add picture boxes
$left = 0
foreach($image in $images) {
	$box = New-Object System.Windows.Forms.PictureBox
	$bitmap = $image.Bitmap
	$box.Image = $bitmap
	$box.SizeMode = 'Zoom'
	$box.Size = New-Object System.Drawing.Size ($bitmap.Size.Width * $scale), ($bitmap.Size.Height * $scale)
	$box.Left = $left
	$left = $box.Right
	if ($images.Count -eq 1) {
		$box.Dock = 'Fill'
	}
	else {
		$box.BorderStyle = 'FixedSingle'
		$box.add_Click({
			foreach($image in $images) {
				if ($image.Bitmap -eq $this.Image) {
					Show-Image $image
					break
				}
			}
		})
	}
	$form.Controls.Add($box)
}

# show!
[void]$form.ShowDialog()

# clean
foreach($image in $images) {
	if ($files -notcontains $image) {
		$image.Bitmap.Dispose()
	}
}
