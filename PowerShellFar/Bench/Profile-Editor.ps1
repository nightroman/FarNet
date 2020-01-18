<#
.Synopsis
	Editor profile (example).
	Author: Roman Kuzmin

.Description
	The editor profile: %FARPROFILE%\FarNet\PowerShellFar\Profile-Editor.ps1
	Before using read this code and "Profile-Editor.ps1" in HLF or HTM help.
#>

### Init focus data, also prevent not allowed next calls.
New-Variable Editor.Data @{} -Scope Global -Option ReadOnly -Description 'Editor handlers data.' -ErrorAction Stop

### GotFocus: reset focus data
$Far.AnyEditor.add_GotFocus({
	${Editor.Data}.Clear()
})

### Customise some file types
$Far.AnyEditor.add_Opened({
	$ext = [System.IO.Path]::GetExtension($this.FileName)
	### Markdown: [F1] ~ preview help
	if ($ext -eq '.md' -or $ext -eq '.text') {
		$this.add_KeyDown({
			if ($_.Key.Is([FarNet.KeyCode]::F1)) {
				$_.Ignore = $true
				Show-Markdown-.ps1 -Help
			}
		})
	}
	### HLF: [F1] ~ preview help
	elseif ($ext -eq '.hlf') {
		$this.add_KeyDown({
			if ($_.Key.Is([FarNet.KeyCode]::F1)) {
				$_.Ignore = $true
				Show-Hlf-.ps1
			}
		})
	}
})

### Mouse click handler
$Far.AnyEditor.add_MouseClick({
	$m = $_.Mouse
	### Left click
	if ($m.Buttons -eq 'Left') {
		if ($m.Is()) {
			# just click, keep the position
			${Editor.Data}.LCPos = $this.ConvertPointScreenToEditor($m.Where)
		}
		elseif ($m.IsShift()) {
			# Shift click, select text from the last to current position
			$_.Ignore = $true
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
		if ($m.Is()) {
			# just click, show the menu
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
})

### Mouse move handler
$Far.AnyEditor.add_MouseMove({
	$m = $_.Mouse
	### Left move
	if ($m.Buttons -eq 'Left') {
		if ($m.Is()) {
			# drag, select text from the last to current position
			$p1 = ${Editor.Data}.LCPos
			if ($p1) {
				$_.Ignore = $true
				$p2 = $this.ConvertPointScreenToEditor($m.Where)
				$this.SelectText($p1.X, $p1.Y, $p2.X, $p2.Y)
				$this.Redraw()
			}
		}
	}
})
