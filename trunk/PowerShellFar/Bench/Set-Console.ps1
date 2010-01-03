
<#
.SYNOPSIS
	Sets console window properties.
	Author: Roman Kuzmin

.DESCRIPTION
	Console resizing is an awkward procedure. This script makes it easier: it
	sets new width and\or height values or resizes window interactively with
	arrow keys.

	Buffer width is always set equal to window width. Buffer height is kept
	equal to width if it is originally equal or it is not changed otherwise.

.EXAMPLE
	# Starts interactive resizing with arrow keys:
	Set-Console.ps1
.EXAMPLE
	# Sets classic small console size:
	Set-Console.ps1 80 25
.EXAMPLE
	# Sets only new width:
	Set-Console.ps1 80
#>

param
(
	# New width. Default: 0.
	$Width = 0,
	# New height. Default: 0.
	$Height = 0
)

$ErrorActionPreference = 'SilentlyContinue'

$ui = $Host.UI.RawUI
$eq = $ui.BufferSize -eq $ui.WindowSize

function NewSize($Width, $Height)
{
	New-Object System.Management.Automation.Host.Size $Width, $Height
}

function SetSize($Width, $Height)
{
	# reduce width
	if ($Width -lt $ui.WindowSize.Width -and $Width -gt 0) {
		$ui.WindowSize = NewSize $Width $ui.WindowSize.Height
		$ui.BufferSize = NewSize $Width $ui.BufferSize.Height
	}
	# increase width
	elseif ($Width -gt $ui.WindowSize.Width) {
		$ui.BufferSize = NewSize $Width $ui.BufferSize.Height
		$ui.WindowSize = NewSize $Width $ui.WindowSize.Height
	}

	# reduce height
	if ($Height -lt $ui.WindowSize.Height -and $Height -gt 0) {
		$ui.WindowSize = NewSize $ui.WindowSize.Width $Height
		if ($eq) {
			$ui.BufferSize = $ui.WindowSize
		}
	}
	# increase height
	elseif ($Height -gt $ui.WindowSize.Height) {
		if ($Height -gt $ui.BufferSize.Height) {
			$ui.BufferSize = NewSize $ui.BufferSize.Width $Height
		}
		$ui.WindowSize = NewSize $ui.WindowSize.Width $Height
	}

	# sync buffer
	if ($eq -and ($ui.BufferSize -ne $ui.WindowSize)) {
		$ui.BufferSize = $ui.WindowSize
	}
}

### Set specified size
if ($Width -gt 0 -or $Height -gt 0) {
	SetSize $Width $Height
	return
}

### Interactive sizing
$title = $ui.WindowTitle
for(;;) {
	$ui.WindowTitle = '{0} x {1} Arrow keys: resize; other keys: exit ...' -f $ui.WindowSize.Width, $ui.WindowSize.Height
	switch($ui.ReadKey(6).VirtualKeyCode) {
		37 {
			SetSize ($ui.WindowSize.Width - 1) $ui.WindowSize.Height
			break
		}
		39 {
			SetSize ($ui.WindowSize.Width + 1) $ui.WindowSize.Height
			break
		}
		38 {
			SetSize $ui.WindowSize.Width ($ui.WindowSize.Height - 1)
			break
		}
		40 {
			SetSize $ui.WindowSize.Width ($ui.WindowSize.Height + 1)
			break
		}
		default {
			$ui.WindowTitle = $title
			return
		}
	}
}
