
<#
.SYNOPSIS
	Editor startup code (example).
	Author: Roman Kuzmin

.DESCRIPTION
	This is an example of configuration "Editor startup code" which is invoked
	when an editor is opened the first time. It installs some key and mouse
	handles. Read help "Profile-Editor-.ps1" before using.
#>

$ErrorActionPreference = 'Stop'

### Install editor data. This line also denies the second call of this script.
New-Variable Editor.Data @{} -Scope Global -Option ReadOnly -Description 'Editor handlers data.'

### Install key handler
$Far.AnyEditor.add_OnKey({&{
	if ($_.Key.KeyDown) {
		$e = $_
		if ($e.Key.ControlKeyState -band [FarNet.ControlKeyStates]::EnhancedKey) {
			# keys:
			if ($e.Key.VirtualKeyCode -eq [FarNet.VKeyCode]::Home) {
				### Go to extended home
				if ($e.Key.CtrlAltShift -eq 0) {
					$e.Ignore = $true
					Go-Home-
					$this.Redraw()
				}
				### Select to extended Home
				elseif ($e.Key.CtrlAltShift -eq [FarNet.ControlKeyStates]::ShiftPressed) {
					$e.Ignore = $true
					Go-Home- -Select
					$this.Redraw()
				}
			}
		}
		else {
			### F1
			if ($_.Key.VirtualKeyCode -eq [FarNet.VKeyCode]::F1) {
				if ($e.Key.CtrlAltShift -eq 0) {
					if ($this.FileName -like '*.hlf') {
						$e.Ignore = $true
						Show-Hlf-
					}
				}
			}
		}
	}
}})

# Install mouse handler
$Far.AnyEditor.add_OnMouse({&{
	# '$_' is the event argument here but inside of 'switch' '$_' is different;
	# that is why we have to keep reference to $_ in a variable (yes, it is odd).
	$e = $_
	$m = $_.Mouse

	if ($m.Action -eq 'Click') {
		### Left click
		if ($m.Buttons -eq 'Left') {
			if ($m.CtrlAltShift -eq 0) {
				${Editor.Data}.LCPos = $this.ConvertPointScreenToEditor($m.Where)
				${Editor.Data}.LMFoo = 1
			}
			elseif ($m.CtrlAltShift -eq [FarNet.ControlKeyStates]::ShiftPressed) {
				$e.Ignore = $true
				${Editor.Data}.LMFoo = 1
				$p1 = ${Editor.Data}.LCPos
				if (!$p1) {
					$p1 = $this.Caret
				}
				$p2 = $this.ConvertPointScreenToEditor($m.Where)
				$this.SelectText($p1.X, $p1.Y, $p2.X, $p2.Y)
				$this.Redraw()
			}
		}
		### Right click
		elseif ($m.Buttons -eq 'Right') {
			if ($m.CtrlAltShift -eq 0) {
				$e.Ignore = $true
				$Editor = $this
				$SelectionExists = $this.SelectionExists
				New-FarMenu -Show -AutoAssignHotkeys -X $m.Where.X -Y $m.Where.Y -Items @(
					New-FarItem 'Cut' { $Far.CopyToClipboard($Editor.GetSelectedText()); $Editor.DeleteText() } -Disabled:(!$SelectionExists)
					New-FarItem 'Copy' { $Far.CopyToClipboard($Editor.GetSelectedText()) } -Disabled:(!$SelectionExists)
					New-FarItem 'Paste' { if ($SelectionExists) { $Editor.DeleteText() } $Editor.InsertText($Far.PasteFromClipboard()) }
				)
			}
		}
	}
	elseif ($m.Action -eq 'Moved') {
		### Left moved
		if ($m.Buttons -eq 'Left') {
			# [_090406_225257] skip the 1st move after some mouse actions
			#  ??? workaround, to remove when fixed in Far or FarNet
			if (${Editor.Data}.LMFoo) {
				$e.Ignore = $true
				${Editor.Data}.LMFoo = 0
			}
			elseif ($m.CtrlAltShift -eq 0) {
				$p1 = ${Editor.Data}.LCPos
				if ($p1) {
					$e.Ignore = $true
					$p2 = $this.ConvertPointScreenToEditor($m.Where)
					$this.SelectText($p1.X, $p1.Y, $p2.X, $p2.Y)
					$this.Redraw()
				}
			}
		}
	}
}})

# Install GotFocus handler. It resets old data.
$Far.AnyEditor.add_GotFocus({&{
	${Editor.Data}.Clear()
}})
