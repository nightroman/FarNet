<#
.Synopsis
	Creates the pure FarNet panel for tests.

.Description
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

param(
	[switch]$NoDots
)

### User data: an object with members that will be used when the panel works
$Data = [PSCustomObject]@{
	AcceptFiles = 0
	DeleteFiles = 0
	CreateFile = 0
	GetContent = 0
	Total = 0
}

### Add a method that updates the panel info on events
$Data | Add-Member -Name UpdateInfo -MemberType ScriptMethod -Value {
	++$this.Total

	# panel, skip unknown
	$Panel = $Far.Panel
	if ($Panel.Explorer.TypeId -ne 'd797d742-3b57-4bfd-a997-da83ba66b9bb') {
		return
	}

	# generate new key labels
	$Panel.SetKeyBars(@(
		[FarNet.KeyBar]::new([FarNet.KeyCode]::F1, 0, $this.Total, '')
		[FarNet.KeyBar]::new([FarNet.KeyCode]::F1, 'LeftCtrlPressed', ('C' + $this.Total), '')
		[FarNet.KeyBar]::new([FarNet.KeyCode]::F1, 'LeftCtrlPressed, LeftAltPressed', ('CA' + $this.Total), '')
		[FarNet.KeyBar]::new([FarNet.KeyCode]::F1, 'LeftCtrlPressed, ShiftPressed', ('CS' + $this.Total), '')
		[FarNet.KeyBar]::new([FarNet.KeyCode]::F1, 'LeftAltPressed', ('A' + $this.Total), '')
		[FarNet.KeyBar]::new([FarNet.KeyCode]::F1, 'LeftAltPressed, ShiftPressed', ('AS' + $this.Total), '')
		[FarNet.KeyBar]::new([FarNet.KeyCode]::F1, 'ShiftPressed', ('S' + $this.Total), '')
	))

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
$Explorer = [PowerShellFar.PowerExplorer]::new('d797d742-3b57-4bfd-a997-da83ba66b9bb')
$Explorer.Functions = 'AcceptFiles, DeleteFiles, ExportFiles, ImportFiles, GetContent, CreateFile'
$Explorer.Data = $Data

### AcceptFiles: called on [F5], [F6]
$Explorer.AsAcceptFiles = {
	param($Explorer, $_)
	# count events, update info
	$data = $Explorer.Data
	++$data.AcceptFiles
	$data.UpdateInfo()

	# just add input files
	$_.Files | %{ $Explorer.Cache.Add($_) }

	# the core deletes on move
	$_.ToDeleteFiles = $true
}

### DeleteFiles: called on [F8]
$Explorer.AsDeleteFiles = {
	param($Explorer, $_)
	# count events, update info
	$data = $Explorer.Data
	++$data.DeleteFiles
	$data.UpdateInfo()

	# remove input files
	$_.Files | % { $Explorer.Cache.Remove($_) }
}

### ExportFiles: [F5], [F6] to a native panel
$Explorer.AsExportFiles = {
	param($Explorer, $_)
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
$Explorer.AsImportFiles = {
	param($Explorer, $_)
	# just add input files
	$_.Files | %{ $Explorer.Cache.Add($_) }
}

### GetContent: called on [F3], [F4], [CtrlQ]
$Explorer.AsGetContent = {
	param($Explorer, $_)
	# count events, update info
	$data = $Explorer.Data
	++$data.GetContent
	$data.UpdateInfo()

	# write content to the file
	[IO.File]::WriteAllText($_.FileName, "Hello from $($_.File.Name)")
}

### CreateFile: called on [F7]
$Explorer.AsCreateFile = {
	param($Explorer, $_)
	# count events, update info
	$data = $Explorer.Data
	$n = ++$data.CreateFile
	$data.UpdateInfo()

	# get and post a new name, add a new item
	$newName = "Item$n"
	$_.PostName = $newName
	$Explorer.Cache.Add((
		New-FarFile $newName -Owner "Value$n" -Description "Description$n" -Columns "custom[0]=$n", "custom[1]=$n"
	))
}

### Create a panel, set its properties
$Panel = [FarNet.Panel]::new($Explorer)
$Panel.DotsMode = if ($NoDots) { 'Off' } else { 'Dots' }
$Panel.DotsDescription = 'Try: F7, F8, F5, F6, CtrlQ, CtrlL, CtrlB, Ctrl7, Ctrl0, Esc'
$Panel.Title = "Test Panel"
$Panel.ViewMode = 'Descriptions'

### Modes
# 'Descriptions'
$Mode = [FarNet.PanelPlan]::new()
$cn = [FarNet.SetColumn]@{ Kind = "N"; Name = "Name (custom title)" }
$co = [FarNet.SetColumn]@{ Kind = "O"; Name = "Owner (custom title)" }
$cz = [FarNet.SetColumn]@{ Kind = "Z"; Name = "Description (custom title)" }
$Mode.Columns = $cn, $co, $cz
$Mode.StatusColumns = $Mode.Columns
$Panel.SetPlan('Descriptions', $Mode)
# 'LongDescriptions'
$Mode = $Mode.Clone()
$Mode.IsFullScreen = $true
$Panel.SetPlan('LongDescriptions', $Mode)
# 'Ctrl0 mode'
$Mode = [FarNet.PanelPlan]::new()
$c0 = [FarNet.SetColumn]@{ Kind = "C0"; Name = "Custom column C0" }
$c1 = [FarNet.SetColumn]@{ Kind = "C1"; Name = "Custom column C1" }
$Mode.Columns = $cn, $c0, $c1
$Mode.StatusColumns = $Mode.Columns
$Panel.SetPlan(0, $Mode)

### Info items
# This test: use info items for event statistics
$Panel.InfoItems = @(
	[FarNet.DataItem]::new('Test Panel Events', $null)
	[FarNet.DataItem]::new('AcceptFiles', 0)
	[FarNet.DataItem]::new('DeleteFiles', 0)
	[FarNet.DataItem]::new('CreateFile', 0)
	[FarNet.DataItem]::new('GetContent', 0)
	[FarNet.DataItem]::new('Last Event', '')
)

### Escaping: closes the panel
$Panel.add_Escaping({
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
})

### KeyPressed
# Optionally process [F1]
$Panel.add_KeyPressed({
	# case [F1]:
	if ($_.Key.Is([FarNet.KeyCode]::F1)) {
		if (0 -eq (Show-FarMessage "[F1] has been pressed" "KeyPressed" -Choices '&Handle', '&Default')) {
			$_.Ignore = $true
			Show-FarMessage "[F1] has been handled" "KeyPressed"
		}
	}
})

# Go!
$Panel.Open()
