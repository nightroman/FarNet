
<#
.SYNOPSIS
	Test pure FarNet panels.
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
	- [CtrlQ] - shows quick view of panel items
	- [CtrlL] - shows event statistics in the info panel
	- [CtrlB] - shows changed key bar labels on each event
	- [Ctrl7], [Ctrl0] - other panel view modes, [Ctrl0] shows custom columns
	- [Esc] - closes a panel with a dialog
#>

param
(
	[switch]$NoDots
)

### Create a panel, set its properties
$p = $Far.CreatePanel()
$p.AddDots = !$NoDots
$p.DotsDescription = 'Try: F7, F8, F5, F6, CtrlQ, CtrlL, CtrlB, Ctrl7, Ctrl0, Esc'
$p.Info.Title = "Test Panel"
$p.Info.StartViewMode = 'Descriptions'

### Modes
# 'Descriptions'
$m = New-Object FarNet.PanelModeInfo
$cn = New-Object FarNet.SetColumn -Property @{ Type = "N"; Name = "Name (custom title)" }
$co = New-Object FarNet.SetColumn -Property @{ Type = "O"; Name = "Owner (custom title)" }
$cz = New-Object FarNet.SetColumn -Property @{ Type = "Z"; Name = "Description (custom title)" }
$m.Columns = $cn, $co, $cz
$m.StatusColumns = $m.Columns
$p.Info.SetMode('Descriptions', $m)
# 'LongDescriptions'
$m = $m.Clone()
$m.IsFullScreen = $true
$p.Info.SetMode('LongDescriptions', $m)
# 'Ctrl0 mode'
$m = New-Object FarNet.PanelModeInfo
$c0 = New-Object FarNet.SetColumn -Property @{ Type = "C0"; Name = "Custom column C0" }
$c1 = New-Object FarNet.SetColumn -Property @{ Type = "C1"; Name = "Custom column C1" }
$m.Columns = $cn, $c0, $c1
$m.StatusColumns = $m.Columns
$p.Info.SetMode(0, $m)

### Info items
# This test: use info items for event statistics
$p.Info.InfoItems = @(
	New-Object FarNet.DataItem 'Test Panel Events', $null
	New-Object FarNet.DataItem 'MakingDirectory', 0
	New-Object FarNet.DataItem 'DeletingFiles', 0
	New-Object FarNet.DataItem 'GettingFiles', 0
	New-Object FarNet.DataItem 'PuttingFiles', 0
	New-Object FarNet.DataItem 'Last Event', ''
)

### Create and keep user data - an object with properties that will be used when the panel is shown
$p.Data = New-Object PSObject -Property @{
	MakingDirectory = 0
	DeletingFiles = 0
	GettingFiles = 0
	PuttingFiles = 0
	Total = 0
	Panel = $p
}

### Add an extra method that updates the panel info on events
$p.Data | Add-Member ScriptMethod UpdateInfo {

	++$this.Total

	# generator of new key labels
	function Make12($s) { for($i = 1; $i -le 12; ++$i) { "$i $s $($this.Total)" } }

	# generate new key labels
	$pi = $this.Panel.Info
	$pi.SetKeyBarAlt((Make12 Alt))
	$pi.SetKeyBarAltShift((Make12 AltShift))
	$pi.SetKeyBarCtrl((Make12 Ctrl))
	$pi.SetKeyBarCtrlAlt((Make12 CtrlAlt))
	$pi.SetKeyBarCtrlShift((Make12 CtrlShift))
	$pi.SetKeyBarMain((Make12 Main))
	$pi.SetKeyBarShift((Make12 Shift))

	# update event counters and reset the property
	$InfoItems = $this.Panel.Info.InfoItems
	$InfoItems[1].Data = $this.MakingDirectory
	$InfoItems[2].Data = $this.DeletingFiles
	$InfoItems[3].Data = $this.GettingFiles
	$InfoItems[4].Data = $this.PuttingFiles
	$InfoItems[5].Data = Get-Date
	$this.Panel.Info.InfoItems = $InfoItems
}

### MakingDirectory: called on [F7]
$p.add_MakingDirectory({&{

	# count events and update the info
	$n = ++$this.Data.MakingDirectory
	$this.Data.UpdateInfo()

	# ignore silent mode in this demo
	if ($_.Mode -band [FarNet.OperationModes]::Silent) {
		$_.Ignore = $true
		return
	}

	# get and return a new item name, add a new item
	$_.Name = "Item$n"
	$this.Files.Add((New-FarFile $_.Name -Owner "Value$n" -Description "Description$n" -Columns "custom[0]=$n", "custom[1]=$n"))
}})

### DeletingFiles: called on [F8]
$p.add_DeletingFiles({&{

	# count events and update the info
	++$this.Data.DeletingFiles
	$this.Data.UpdateInfo()

	# remove input files
	foreach($f in $_.Files) {
		$this.Files.Remove($f)
	}
}})

### GettingFiles: called on [F5], [CtrlQ]
$p.add_GettingFiles({&{

	# count events and update the info
	++$this.Data.GettingFiles
	$this.Data.UpdateInfo()

	# case: [CtrlQ]
	if ($_.Mode -band [FarNet.OperationModes]::QuickView) {
		$name = $_.Files[0].Name
		[IO.File]::WriteAllText("$($_.Destination)\$name", "Hello from $name")
		return
	}

	# other cases
	$that = $Far.Panel2
	if (!$that.Info -or $That.Info.Title -ne $this.Info.Title) {
		$Far.Message('Open another test panel for this operation.')
	}
}})

### PuttingFiles: called on [F6]
$p.add_PuttingFiles({&{

	# count events and update the info
	++$this.Data.PuttingFiles
	$this.Data.UpdateInfo()

	# process input files: just add them in this demo
	foreach($f1 in $_.Files) {
		$this.Files.Add((New-FarFile -Name $f1.Name))
	}
}})

### Escaping: closes the panel
$p.add_Escaping({&{

	# set processed
	$_.Ignore = $true

	# prompt
	switch(Show-FarMessage 'How to close this panel?' -Choices '&1:Close()', '&2:Close(ActivePath)', 'Cancel') {
		0 {
			# [_090321_210416]: Far panel current item depends on the plugin panel current item (fixed in FarNet)
			$this.Close()
		}
		1 {
			$this.Close($this.ActivePath)
		}
	}
}})

### KeyPressed: shows how to process some keys
$p.add_KeyPressed({&{
	if (!$_.Preprocess) {
		# case [F1]:
		if ($_.Code -eq [FarNet.VKeyCode]::F1 -and $_.State -eq 0) {
			if (0 -eq (Show-FarMessage "[F1] has been pressed" -Choices 'Process by handler', 'Allow default action')) {
				$_.Ignore = $true
				Show-FarMessage "[F1] has been pressed and processed by the handler"
			}
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
$p.add_Closing({&{
	$DebugPanelClosing = $true
}})

# Go!
$p.Open()
