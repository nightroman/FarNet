
<#
.SYNOPSIS
	Shows image(s) in a GUI window.

.DESCRIPTION
	This script shows pictures in a very simple way. It does does not require
	any additional tools. It is convenient to use from Far Manager via file
	associations or user menu commands, see examples.

	The script can be called directly but in this case its modal dialog blocks
	the calling thread. To avoid this this script should be called as a job or
	by starting a separate PowerShell process in a hidden window.

	For a single image the window is resizable. More than one input images are
	placed from left to right and scaled, if needed; mouse click on a picture
	opens a separate modal window with this picture, [Enter] opens the current
	picture, [Left] and [Right] changes the current picture. [Escape] closes
	the window.

.INPUTS
	Image file paths are passed in as arguments or piped. If there is no input
	then all image files from the current location are taken.

.EXAMPLE
	# Far Manager association: internal way: faster but picture windows will be
	# closed together with the Far window on exit.
	>: Start-FarJob -Hidden Show-Image (Get-FarPath) #

.EXAMPLE
	# Far Manager association: external way: slower but picture windows will be
	# opened even after closing the Far window.
	start /min powershell -WindowStyle Hidden -File C:\Scripts\Show-Image.ps1 "!\!.!"

.EXAMPLE
	# Far Manager user menu: internal way: show selected images
	>: Start-FarJob -Hidden Show-Image (Get-FarPath -Selected) #

.EXAMPLE
	# Far Manager user menu: external way: show all images here
	start /min powershell -WindowStyle Hidden Show-Image
#>

param([switch]$Internal)
Set-StrictMode -Version 2
Add-Type -AssemblyName System.Windows.Forms

### input, create bitmaps
$files = if ($args) { $args } else { $input }
if (!$files) { $files = Get-Item *.bmp,*.gif,*.jpg,*.jpeg,*.png,*.tif,*.tiff,*.wmf }
$width = 0
$height = 0
$images = New-Object System.Collections.ArrayList
foreach($file in $files) {
	try {
		$path = $file.ToString()
		$bitmap = New-Object System.Drawing.Bitmap $path
		$image = @{
			Path = $path
			Bitmap = $bitmap
		}
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
$form.Text = ($images | .{process{ [System.IO.Path]::GetFileName($_.Path) }}) -join ', '
$form.BackColor = [System.Drawing.Color]::FromArgb(0, 0, 0)
if ($images.Count -eq 1) {
	$form.StartPosition = 'CenterScreen'
	$form.FormBorderStyle = 'Sizable'
	$bordersize = [System.Windows.Forms.SystemInformation]::FrameBorderSize
	$form.MaximizeBox = $true
}
else {
	$form.StartPosition = 'Manual'
	$form.FormBorderStyle = 'FixedDialog'
	$bordersize = [System.Windows.Forms.SystemInformation]::FixedFrameBorderSize
	$form.MaximizeBox = $false
}
$form.add_Shown({ $form.Activate() })
$form.add_KeyDown({ . KeyDown })

### get scale factor and set form size
$scale = 1
$maxsize = [System.Windows.Forms.SystemInformation]::WorkingArea
$maxwidth = $maxsize.Width - 2 * $bordersize.Width
if ($width -gt $maxwidth) {
	$scale = $maxwidth / $width
	$width = $width * $scale
	$height = $height * $scale
}
$maxheight = $maxsize.Height - $bordersize.Height - [System.Windows.Forms.SystemInformation]::CaptionHeight
if ($height -gt $maxheight) {
	$scale = $maxheight / $height
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
		$box.BorderStyle = 'None'
		$box.add_Click({
			. Click
		})
	}
	$form.Controls.Add($box)
}

if ($images.Count -ne 1) {
	$current = 0
	$mainform = $form
	$form.Controls[0].BorderStyle = 'Fixed3D'
}

function KeyDown {
	switch($_.KeyCode) {
		'Escape' {
			$this.Close()
		}
		'Return' {
			if ($images.Count -ne 1) {
				. ShowCurrent
			}
		}
		'Left' {
			if ($images.Count -ne 1) {
				. Left
			}
			elseif ($Internal) {
				$action.Value = 'Left'
				$this.Close()
			}
		}
		'Right' {
			if ($images.Count -ne 1) {
				. Right
			}
			elseif ($Internal) {
				$action.Value = 'Right'
				$this.Close()
			}
		}
	}
}

function Left {
	$box = $mainform.Controls[$current]
	$box.BorderStyle = 'None'
	--$current
	if ($current -lt 0) { $current = $images.Count - 1 }
	$box = $mainform.Controls[$current]
	$box.BorderStyle = 'Fixed3D'
}

function Right {
	$box = $mainform.Controls[$current]
	$box.BorderStyle = 'None'
	++$current
	if ($current -ge $images.Count) { $current = 0 }
	$box = $mainform.Controls[$current]
	$box.BorderStyle = 'Fixed3D'
}

function Click {
	for($e = 0; $e -lt $images.Count; ++$e) {
		if ($images[$e].Bitmap -eq $this.Image) {
			break
		}
	}
	$box = $mainform.Controls[$current]
	$box.BorderStyle = 'None'
	$current = $e
	$box = $mainform.Controls[$current]
	$box.BorderStyle = 'Fixed3D'
	. ShowCurrent
}

function ShowCurrent {
	for(;;) {
		$action = [ref]$null
		Show-Image -Internal ($images[$current].Path)
		switch($action.Value) {
			'Left' {
				. Left
			}
			'Right' {
				. Right
			}
			default {
				return
			}
		}
	}
}

### show
[void]$form.ShowDialog()
$form.Dispose()

### dispose
foreach($_ in $images) {
	$_.Bitmap.Dispose()
}
