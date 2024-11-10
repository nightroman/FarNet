<#
.Synopsis
	Shows one or more images in a GUI window.
	Author: Roman Kuzmin

.Description
	This script shows pictures in a GUI window. It is a standard PowerShell
	script but it may be easier to use in Far Manager via file associations
	or user menu commands, see examples.

	The script may be called directly. In this case its modal dialog blocks
	the calling thread. To avoid this the script may be called as a job or a
	separate PowerShell process with a hidden window.

	For a single image its window is resizable, [Enter] switches maximized and
	normal state. Several images are placed from left to right and scaled, if
	needed; mouse click on a picture opens a window with this picture, [Enter]
	opens the current picture, [Left] and [Right] changes the current picture.
	[Escape] closes the window.

.Inputs
	Image file paths are passed in as arguments or piped. If there is no input
	then all image files from the current location are taken.

.Example
	> Start-FarJob -Hidden Show-Image (Get-FarPath)

	Far Manager association: internal way: faster but picture windows will be
	closed together with the Far window on exit.

.Example
	start /min powershell -WindowStyle Hidden -File C:\PS\Show-Image.ps1 "!\!.!"

	Far Manager association: external way: slower but picture windows will be
	opened even after closing the Far window.

.Example
	> Start-FarJob -Hidden Show-Image (Get-FarPath -Selected)

	Far Manager user menu: internal way: show selected images.

.Example
	> start /min powershell -WindowStyle Hidden Show-Image

	Far Manager user menu: external way: show all images here.
#>

param(
	[switch]$Internal
)

Set-StrictMode -Version 3
Add-Type -AssemblyName System.Windows.Forms

### input, create bitmaps, get total width and height
$files = if ($args) { $args } else { $input }
if (!$files) {
	$files = Get-Item *.bmp,*.gif,*.jpg,*.jpeg,*.png,*.tif,*.tiff,*.wmf
}
$width = 0
$height = 0
$images = [System.Collections.ArrayList]::new()
foreach($file in $files) {
	try {
		$path = $file.ToString()
		$bitmap = [System.Drawing.Bitmap]::new($path)
		$width += $bitmap.Size.Width
		if ($height -lt $bitmap.Size.Height) {
			$height = $bitmap.Size.Height
		}
		$null = $images.Add(@{ Path = $path; Bitmap = $bitmap })
	}
	catch {}
}
if ($images.Count -eq 0) {
	return
}

### create a form
$form = [System.Windows.Forms.Form]::new()
$form.Text = ($images | .{process{ [System.IO.Path]::GetFileName($_.Path) }}) -join ', '
$form.BackColor = [System.Drawing.Color]::FromArgb(0, 0, 0)
$form.add_Shown({ $form.Activate() })
$form.add_KeyDown({ . KeyDown })
if ($images.Count -eq 1) {
	$form.MaximizeBox = $true
	$form.StartPosition = 'CenterScreen'
	$form.FormBorderStyle = 'Sizable'
	$bordersize = [System.Windows.Forms.SystemInformation]::FrameBorderSize
}
else {
	$form.MaximizeBox = $false
	$form.StartPosition = 'Manual'
	$form.FormBorderStyle = 'FixedDialog'
	$bordersize = [System.Windows.Forms.SystemInformation]::FixedFrameBorderSize
}
if ($Internal) {
	$form.WindowState = $WindowState.Value
}

### get scale factor, calculate and set form client size
$scale = 1
$maxsize = [System.Windows.Forms.SystemInformation]::WorkingArea
$maxwidth = $maxsize.Width - 2 * $bordersize.Width
if ($width -gt $maxwidth) {
	$scale = $maxwidth / $width
	$width *= $scale
	$height *= $scale
}
$maxheight = $maxsize.Height - $bordersize.Height - [System.Windows.Forms.SystemInformation]::CaptionHeight
if ($height -gt $maxheight) {
	$scale2 = $maxheight / $height
	$scale *= $scale2
	$width *= $scale2
	$height *= $scale2
}
$form.ClientSize = [System.Drawing.Size]::new($width, $height)

### create and add picture boxes
$left = 0
foreach($image in $images) {
	$bitmap = $image.Bitmap
	$box = [System.Windows.Forms.PictureBox]::new()
	if ($images.Count -eq 1) {
		$box.Dock = 'Fill'
	}
	else {
		$box.add_Click({ . Click })
		if ($Left -eq 0) {
			### init the current picture box
			$box.BorderStyle = 'Fixed3D'
			$script:current = 0
			$script:mainform = $form
		}
	}
	$box.Image = $bitmap
	$box.SizeMode = 'Zoom'
	$box.Size = [System.Drawing.Size]::new(($bitmap.Size.Width * $scale), ($bitmap.Size.Height * $scale))
	$box.Left = $left
	$left = $box.Right
	$form.Controls.Add($box)
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
			else {
				switch($this.WindowState) {
					'Normal' { $this.WindowState = 'Maximized' }
					'Maximized' { $this.WindowState = 'Normal' }
				}
			}
		}
		'Left' {
			if ($images.Count -ne 1) {
				. Left
			}
			elseif ($Internal) {
				$Action.Value = 'Left'
				$WindowState.Value = $this.WindowState
				$this.Close()
			}
		}
		'Right' {
			if ($images.Count -ne 1) {
				. Right
			}
			elseif ($Internal) {
				$Action.Value = 'Right'
				$WindowState.Value = $this.WindowState
				$this.Close()
			}
		}
	}
}

function Left {
	$index = $script:current - 1
	if ($index -lt 0) {
		$index = $images.Count - 1
	}
	. SetCurrent $index
}

function Right {
	$index = $script:current + 1
	if ($index -ge $images.Count) {
		$index = 0
	}
	. SetCurrent $index
}

function Click {
	for($index = 0; $index -lt $images.Count; ++$index) {
		if ($images[$index].Bitmap -eq $this.Image) {
			break
		}
	}
	. SetCurrent $index
	. ShowCurrent
}

function SetCurrent($index) {
	$script:mainform.Controls[$script:current].BorderStyle = 'None'
	$script:mainform.Controls[$index].BorderStyle = 'Fixed3D'
	$script:current = $index
}

function ShowCurrent {
	$WindowState = [ref]'Normal'
	for(;;) {
		$Action = [ref]$null
		Show-Image -Internal ($images[$script:current].Path)
		switch($Action.Value) {
			'Left' { . Left }
			'Right' { . Right }
			default { return }
		}
	}
}

### show the modal dialog
[void]$form.ShowDialog()
$form.Dispose()

### dispose bitmaps
foreach($_ in $images) {
	$_.Bitmap.Dispose()
}
