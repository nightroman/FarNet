<#
.Synopsis
	Shows one or more images in a GUI window.
	Author: Roman Kuzmin

.Description
	This script shows pictures in a GUI window. It is a standard PowerShell
	script but it may be easier to use in Far Manager via file associations
	or user menu commands, see examples.

	The script may be called directly. In this case its modal dialog blocks.
	To avoid this, call the script as a task or a hidden window process.

	A single image window is resizable, [Enter] switches maximized and normal
	state. Several images are placed from left to right and scaled, mouse
	click on an image opens a window with this image, [Enter] opens the
	current image, [Left] and [Right] changes the current image. [Escape]
	closes the window.

.Inputs
	Image file paths are passed in as arguments or piped. If there is no input
	then all image files from the current location are taken.

.Example
	Start-FarTask -File (Get-FarPath) { param($File) Show-Image.ps1 $File }

	Shows the cursor image file using a task. Suitable for file associations.
	The image window is closed if Far Manager exits.

.Example
	Start-FarTask -File (Get-FarPath -Selected) { param($File) Show-Image.ps1 $File }

	Shows selected images or all (on dots) using using a task. Suitable for
	user menus. The image window is closed if Far Manager exits.

.Example
	@start /min powershell -WindowStyle Hidden -Command Show-Image.ps1 "!\!.!"

	Shows the cursor image file using a hidden window process. Suitable for
	file associations. The image window stays opened if Far Manager exits.

.Example
	@start /min powershell -WindowStyle Hidden -Command Show-Image.ps1

	Shows all current directory images using a hidden window process. Suitable
	for user menus. The image window stays opened if Far Manager exits.
#>

param(
	[switch]$Internal
)

Add-Type -AssemblyName System.Windows.Forms

function on_KeyDown {
	$$ = $_.KeyCode

	if ($$ -eq 'Escape') {
		$this.Close()
		return
	}

	if ($$ -eq 'Left') {
		if ($images.Count -ne 1) {
			do_left
		}
		elseif ($Internal) {
			$Action.Value = 'Left'
			$WindowState.Value = $this.WindowState
			$this.Close()
		}
		return
	}

	if ($$ -eq 'Right') {
		if ($images.Count -ne 1) {
			do_right
		}
		elseif ($Internal) {
			$Action.Value = 'Right'
			$WindowState.Value = $this.WindowState
			$this.Close()
		}
		return
	}

	if ($$ -in 'Enter', 'Return') {
		if ($images.Count -ne 1) {
			show_current
		}
		else {
			switch($this.WindowState) {
				'Normal' { $this.WindowState = 'Maximized' }
				'Maximized' { $this.WindowState = 'Normal' }
			}
		}
	}
}

function do_left {
	$index = $script:current - 1
	if ($index -lt 0) {
		$index = $images.Count - 1
	}
	. set_current $index
}

function do_right {
	$index = $script:current + 1
	if ($index -ge $images.Count) {
		$index = 0
	}
	. set_current $index
}

function do_Click {
	for($index = 0; $index -lt $images.Count; ++$index) {
		if ($images[$index].Bitmap -eq $this.Image) {
			break
		}
	}
	. set_current $index
	. show_current
}

function set_current($index) {
	$script:mainform.Controls[$script:current].BorderStyle = 'None'
	$script:mainform.Controls[$index].BorderStyle = 'Fixed3D'
	$script:current = $index
}

function show_current {
	$WindowState = [ref]'Normal'
	for(;;) {
		$Action = [ref]$null
		Show-Image -Internal ($images[$script:current].Path)
		switch($Action.Value) {
			Left { do_left }
			Right { do_right }
			default { return }
		}
	}
}

### input, create bitmaps, get total width and height
$files = if ($args) { $args } else { $input }
if (!$files) {
	$files = Get-Item *.bmp,*.gif,*.jpg,*.jpeg,*.png,*.tif,*.tiff,*.wmf
}
$width = 0
$height = 0
$images = [System.Collections.Generic.List[object]]::new()
foreach($file in $files) {
	try {
		$path = $file.ToString()
		$bitmap = [System.Drawing.Bitmap]::new($path)
		$width += $bitmap.Size.Width
		$height = [System.Math]::Max($height, $bitmap.Size.Height)
		$images.Add(@{ Path = $path; Bitmap = $bitmap })
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
$form.add_KeyDown(${function:on_KeyDown})
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
		$box.add_Click(${function:do_Click})
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

### show the modal dialog
[void]$form.ShowDialog()
$form.Dispose()

### dispose bitmaps
foreach($_ in $images) {
	$_.Bitmap.Dispose()
}
