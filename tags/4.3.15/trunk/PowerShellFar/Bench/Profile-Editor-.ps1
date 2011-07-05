
<#
.SYNOPSIS
	Editor startup code (example).
	Author: Roman Kuzmin

.DESCRIPTION
	This is an example of configuration "Editor startup code" which is invoked
	when an editor is opened the first time. It installs some key and mouse
	handles. Before using read help "Profile-Editor-.ps1" and the code.
#>

$ErrorActionPreference = 'Stop'

### Editor data; this line also denies the second call of this script
New-Variable Editor.Data @{} -Scope Global -Option ReadOnly -Description 'Editor handlers data.'

### GotFocus handler; it resets old data
$Far.AnyEditor.add_GotFocus({&{
	${Editor.Data}.Clear()
}})

### Key down handler
$Far.AnyEditor.add_KeyDown({&{
	$e = $_
	if ($e.Key.ControlKeyState -band [FarNet.ControlKeyStates]::EnhancedKey) {
		# navigation keys (not used now)
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
}})

### Mouse click handler
$Far.AnyEditor.add_MouseClick({&{
	$m = $_.Mouse
	### Left click
	if ($m.Buttons -eq 'Left') {
		if ($m.CtrlAltShift -eq 0) {
			${Editor.Data}.LCPos = $this.ConvertPointScreenToEditor($m.Where)
			${Editor.Data}.LMFoo = 1
		}
		elseif ($m.CtrlAltShift -eq [FarNet.ControlKeyStates]::ShiftPressed) {
			$_.Ignore = $true
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
			$_.Ignore = $true
			$Editor = $this
			$SelectionExists = $this.SelectionExists
			New-FarMenu -Show -AutoAssignHotkeys -X $m.Where.X -Y $m.Where.Y -Items @(
				New-FarItem 'Cut' { $Far.CopyToClipboard($Editor.GetSelectedText()); $Editor.DeleteText() } -Disabled:(!$SelectionExists)
				New-FarItem 'Copy' { $Far.CopyToClipboard($Editor.GetSelectedText()) } -Disabled:(!$SelectionExists)
				New-FarItem 'Paste' { if ($SelectionExists) { $Editor.DeleteText() } $Editor.InsertText($Far.PasteFromClipboard()) }
			)
		}
	}
}})

### Mouse move handler
$Far.AnyEditor.add_MouseMove({&{
	$m = $_.Mouse
	### Left moved
	if ($m.Buttons -eq 'Left') {
		# [_090406_225257] skip the 1st move after some mouse actions
		#  ??? workaround, to remove when fixed in Far or FarNet
		if (${Editor.Data}.LMFoo) {
			$_.Ignore = $true
			${Editor.Data}.LMFoo = 0
		}
		elseif ($m.CtrlAltShift -eq 0) {
			$p1 = ${Editor.Data}.LCPos
			if ($p1) {
				$_.Ignore = $true
				$p2 = $this.ConvertPointScreenToEditor($m.Where)
				$this.SelectText($p1.X, $p1.Y, $p2.X, $p2.Y)
				$this.Redraw()
			}
		}
	}
}})
