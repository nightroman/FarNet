
<#
.SYNOPSIS
	Test simple panels.
	Author: Roman Kuzmin

.DESCRIPTION
	PowerShellFar panels are often derived from AnyPanel, but this is not a
	requirement for scripts. This script shows how to use a pure FarNet panel
	and its features. Other points of interest: custom modes, titles, columns;
	examples of main panel operations.

	How to run:
	- run this script to open a panel
	- try commands [F7], [F8] - 'New', 'Delete'
	- go to passive panel and run this to open another panel

	Try other commands:
	- [F5], [F6] - 'Copy', 'Move'
	- [CtrlQ], [F3], [F4] - quick view, view, edit
	- [CtrlL] - shows event statistics in the info panel
	- [CtrlB] - shows changed key bar labels on each event
	- [Ctrl7], [Ctrl0] - other panel view modes, [Ctrl0] shows custom columns
	- [Esc] - closes a panel with a dialog
#>

param
(
	[switch]$NoDots
)

### User data: an object with members that will be used when the panel works
$Data = New-Object PSObject -Property @{
	AcceptFiles = 0
	DeleteFiles = 0
	CreateFile = 0
	GetContent = 0
	Total = 0
}

### Add a method that updates the panel info on events
$Data | Add-Member ScriptMethod UpdateInfo {
	++$this.Total

	# panel, skip alien
	$Panel = $Far.Panel
	if ($Panel.Explorer.TypeId -ne 'd797d742-3b57-4bfd-a997-da83ba66b9bb') { return }

	# generator of new key labels
	function Make12($s) { for($i = 1; $i -le 12; ++$i) { "$i $s $($this.Total)" } }

	# generate new key labels
	$Panel.SetKeyBar((Make12 Main))
	$Panel.SetKeyBarCtrl((Make12 Ctrl))
	$Panel.SetKeyBarCtrlAlt((Make12 CtrlAlt))
	$Panel.SetKeyBarCtrlShift((Make12 CtrlShift))
	$Panel.SetKeyBarAlt((Make12 Alt))
	$Panel.SetKeyBarAltShift((Make12 AltShift))
	$Panel.SetKeyBarShift((Make12 Shift))

	# update event counters and reset the property
	$InfoItems = $Panel.InfoItems
	$InfoItems[1].Data = $this.AcceptFiles
	$InfoItems[2].Data = $this.DeleteFiles
	$InfoItems[3].Data = $this.CreateFile
	$InfoItems[4].Data = $this.GetContent
	$InfoItems[5].Data = Get-Date
	$Panel.InfoItems = $InfoItems
}

### Explorer
# It does not need the AsGetFiles, it uses the predefined Files list.
$Explorer = New-Object PowerShellFar.PowerExplorer 'd797d742-3b57-4bfd-a997-da83ba66b9bb' -Property @{
	Functions = 'AcceptFiles, DeleteFiles, ExportFiles, ImportFiles, GetContent, CreateFile'
	Data = $Data
	### AcceptFiles: called on [F5], [F6]
	AsAcceptFiles = {
		# count events, update info
		$data = $this.Data
		++$data.AcceptFiles
		$data.UpdateInfo()

		# just add input files
		$_.Files | %{ $this.Cache.Add($_) }

		# the core deletes on move
		$_.ToDeleteFiles = $true
	}
	### DeleteFiles: called on [F8]
	AsDeleteFiles = {
		# count events, update info
		$data = $this.Data
		++$data.DeleteFiles
		$data.UpdateInfo()

		# remove input files
		$_.Files | % { $this.Cache.Remove($_) }
	}
	### ExportFiles: [F5], [F6] to a native panel
	AsExportFiles = {
		# allow delete on move
		$_.ToDeleteFiles = $true
		# make some fake files and simulate incomplete results
		foreach($file in $_.Files) {
			$path = Join-Path $_.DirectoryName $file.Name
			$file | Format-List | Out-File $path -Confirm
			if (![IO.File]::Exists($path)) {
				$_.Result = 'Incomplete'
				$_.FilesToStay.Add($file)
			}
		}
	}
	### ImportFiles: [F5], [F6] from a native panel
	AsImportFiles = {
		# just add input files
		$_.Files | %{ $this.Cache.Add($_) }
	}
	### GetContent: called on [F3], [F4], [CtrlQ]
	AsGetContent = {
		# count events, update info
		$data = $this.Data
		++$data.GetContent
		$data.UpdateInfo()

		# write content to the file
		[IO.File]::WriteAllText($_.FileName, "Hello from $($_.File.Name)")
	}
	### CreateFile: called on [F7]
	AsCreateFile = {
		# count events, update info
		$data = $this.Data
		$n = ++$data.CreateFile
		$data.UpdateInfo()

		# get and post a new name, add a new item
		$newName = "Item$n"
		$_.PostName = $newName
		$this.Cache.Add((
			New-FarFile $newName -Owner "Value$n" -Description "Description$n" -Columns "custom[0]=$n", "custom[1]=$n"
		))
	}
}

### Create a panel, set its properties
$Panel = New-Object FarNet.Panel $Explorer -Property @{
	DotsMode = if ($NoDots) { 'Off' } else { 'Dots' }
	DotsDescription = 'Try: F7, F8, F5, F6, CtrlQ, CtrlL, CtrlB, Ctrl7, Ctrl0, Esc'
	Title = "Test Panel"
	ViewMode = 'Descriptions'
}

### Modes
# 'Descriptions'
$Mode = New-Object FarNet.PanelPlan
$cn = New-Object FarNet.SetColumn -Property @{ Kind = "N"; Name = "Name (custom title)" }
$co = New-Object FarNet.SetColumn -Property @{ Kind = "O"; Name = "Owner (custom title)" }
$cz = New-Object FarNet.SetColumn -Property @{ Kind = "Z"; Name = "Description (custom title)" }
$Mode.Columns = $cn, $co, $cz
$Mode.StatusColumns = $Mode.Columns
$Panel.SetPlan('Descriptions', $Mode)
# 'LongDescriptions'
$Mode = $Mode.Clone()
$Mode.IsFullScreen = $true
$Panel.SetPlan('LongDescriptions', $Mode)
# 'Ctrl0 mode'
$Mode = New-Object FarNet.PanelPlan
$c0 = New-Object FarNet.SetColumn -Property @{ Kind = "C0"; Name = "Custom column C0" }
$c1 = New-Object FarNet.SetColumn -Property @{ Kind = "C1"; Name = "Custom column C1" }
$Mode.Columns = $cn, $c0, $c1
$Mode.StatusColumns = $Mode.Columns
$Panel.SetPlan(0, $Mode)

### Info items
# This test: use info items for event statistics
$Panel.InfoItems = @(
	New-Object FarNet.DataItem 'Test Panel Events', $null
	New-Object FarNet.DataItem 'AcceptFiles', 0
	New-Object FarNet.DataItem 'DeleteFiles', 0
	New-Object FarNet.DataItem 'CreateFile', 0
	New-Object FarNet.DataItem 'GetContent', 0
	New-Object FarNet.DataItem 'Last Event', ''
)

### Escaping: closes the panel
$Panel.add_Escaping({&{

	# set processed
	$_.Ignore = $true

	# prompt
	switch(Show-FarMessage 'How to close this panel?' -Choices '&1:Close()', '&2:Close(StartDirectory)', 'Cancel') {
		0 {
			# [_090321_210416]: Far panel current item depends on the plugin panel current item (fixed in FarNet)
			$this.Close()
		}
		1 {
			$this.Close($this.StartDirectory)
		}
	}
}})

### KeyPressed
# Optionally process [F1]
$Panel.add_KeyPressed({&{
	# case [F1]:
	if ($_.Code -eq [FarNet.VKeyCode]::F1 -and $_.State -eq 0) {
		if (0 -eq (Show-FarMessage "[F1] has been pressed" "KeyPressed" -Choices '&Handle', '&Default')) {
			$_.Ignore = $true
			Show-FarMessage "[F1] has been handled" "KeyPressed"
		}
	}
}})

### Closing:
<#
Far issue [_090321_165608]: how to reproduce:
- set variable breakpoint:
>: Set-PSBreakpoint -Variable DebugPanelClosing
-- open this panel and keep it active
-- invoke a trivial PowerShell command from cmdline:
>: 1+1
-- breakpoint is hit: this is bad, panel is not closing at all!
#>
$Panel.add_Closing({&{
	$DebugPanelClosing = $true
}})

# Go!
$Panel.Open()
