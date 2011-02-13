
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
$Panel = New-Object FarNet.Panel
$Panel.DotsMode = if ($NoDots) { 'Off' } else { 'Dots' }
$Panel.DotsDescription = 'Try: F7, F8, F5, F6, CtrlQ, CtrlL, CtrlB, Ctrl7, Ctrl0, Esc'
$Panel.Title = "Test Panel"
$Panel.ViewMode = 'Descriptions'

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
	New-Object FarNet.DataItem 'MakeDirectory', 0
	New-Object FarNet.DataItem 'DeleteFiles', 0
	New-Object FarNet.DataItem 'ExportFiles', 0
	New-Object FarNet.DataItem 'ImportFiles', 0
	New-Object FarNet.DataItem 'Last Event', ''
)

### Create and keep user data - an object with properties that will be used when the panel is shown
$Panel.Data.Host = New-Object PSObject -Property @{
	MakeDirectory = 0
	DeleteFiles = 0
	ExportFiles = 0
	ImportFiles = 0
	Total = 0
	Panel = $Panel
	KeyHandler = $null
}

### Add an extra method that updates the panel info on events
$Panel.Data.Host | Add-Member ScriptMethod UpdateInfo {

	++$this.Total

	# generator of new key labels
	function Make12($s) { for($i = 1; $i -le 12; ++$i) { "$i $s $($this.Total)" } }

	# generate new key labels
	$this.Panel.SetKeyBar((Make12 Main))
	$this.Panel.SetKeyBarAlt((Make12 Alt))
	$this.Panel.SetKeyBarAltShift((Make12 AltShift))
	$this.Panel.SetKeyBarCtrl((Make12 Ctrl))
	$this.Panel.SetKeyBarCtrlAlt((Make12 CtrlAlt))
	$this.Panel.SetKeyBarCtrlShift((Make12 CtrlShift))
	$this.Panel.SetKeyBarShift((Make12 Shift))

	# update event counters and reset the property
	$InfoItems = $this.Panel.InfoItems
	$InfoItems[1].Data = $this.MakeDirectory
	$InfoItems[2].Data = $this.DeleteFiles
	$InfoItems[3].Data = $this.ExportFiles
	$InfoItems[4].Data = $this.ImportFiles
	$InfoItems[5].Data = Get-Date
	$this.Panel.InfoItems = $InfoItems
}

### MakeDirectory: called on [F7]
$Panel.add_MakeDirectory({&{
	$data = $this.Data.Host

	# count events and update the info
	$n = ++$data.MakeDirectory
	$data.UpdateInfo()

	# ignore silent mode in this demo
	if ($_.Mode -band [FarNet.OperationModes]::Silent) {
		$_.Ignore = $true
		return
	}

	# get and return a new item name, add a new item
	$_.Name = "Item$n"
	$this.Files.Add((New-FarFile $_.Name -Owner "Value$n" -Description "Description$n" -Columns "custom[0]=$n", "custom[1]=$n"))
}})

### DeleteFiles: called on [F8]
$Panel.add_DeleteFiles({&{
	$data = $this.Data.Host

	# count events and update the info
	++$data.DeleteFiles
	$data.UpdateInfo()

	# remove input files
	foreach($f in $_.Files) {
		$this.Files.Remove($f)
	}
}})

### ExportFiles: called on [F5], [CtrlQ]
$Panel.add_ExportFiles({&{
	$data = $this.Data.Host

	# count events and update the info
	++$data.ExportFiles
	$data.UpdateInfo()

	# case: [CtrlQ]
	if ($_.Mode -band [FarNet.OperationModes]::QuickView) {
		$name = $_.Files[0].Name
		[IO.File]::WriteAllText("$($_.Destination)\$name", "Hello from $name")
		return
	}

	# other cases
	$that = $Far.Panel2
	if ($That.Title -ne $this.Title) {
		$Far.Message('Open another test panel for this operation.')
	}
}})

### ImportFiles: called on [F6]
$Panel.add_ImportFiles({&{
	$data = $this.Data.Host

	# count events and update the info
	++$data.ImportFiles
	$data.UpdateInfo()

	# process input files: just add them in this demo
	foreach($f1 in $_.Files) {
		$this.Files.Add((New-FarFile -Name $f1.Name))
	}
}})

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

### The key handler used in KeyPressing and KeyPressed events
# [F1] is sent to both events if KeyPressing does not handle it
$Panel.Data.Host.KeyHandler = {
	# case [F1]:
	if ($_.Code -eq [FarNet.VKeyCode]::F1 -and $_.State -eq 0) {
		if (0 -eq (Show-FarMessage "[F1] has been pressed" $args[0] -Choices '&Handle', '&Default')) {
			$_.Ignore = $true
			Show-FarMessage "[F1] has been handled" $args[0]
		}
	}
}

### KeyPressed: processes some keys.
$Panel.add_KeyPressed({ & $this.Data.Host.KeyHandler 'KeyPressed' })

### KeyPressing: pre-processes some keys.
$Panel.add_KeyPressing({ & $this.Data.Host.KeyHandler 'KeyPressing' })

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
