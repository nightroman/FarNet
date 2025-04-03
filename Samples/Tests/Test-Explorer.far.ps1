<#
.Synopsis
	Test scripted explorers with panels.

.Description
	PowerExplorer is the fully functional explorer designed for scripts. Interface
	methods have related script block properties. Each method calls its script.

	This script shows a complex tree with explorers depending on top nodes.
	Technical details are explained in comments.

	It is important for testing that two explorers use the same source data. As
	a result, the FarNet search panel contains a lot of files with same names.
	This is a tough case for native panels but it works fine in FarNet panels.

.Example
	Examples show how Start-FarSearch works with this panel:

	# Invoke from Flat or use -Recurse from Root:
	ps: Start-FarSearch *help*
	ps: Start-FarSearch { $_.Data.Definition -match 'throw' }

	# Invoke from Root: the 1st uses -Mask, the 2nd uses -Script:
	ps: Start-FarSearch -Recurse -Directory *explore*
	ps: Start-FarSearch -Recurse -Directory { $_.Name -like *explore* }
#>

# Root explorer - complex data tree with different explorers. It works like a
# menu where each item opens a different explorer in its own child panel.
function global:New-TestRootExplorer {
	$Explorer = [PowerShellFar.PowerExplorer]::new('4fba4f3c-00c3-4aa1-be67-893fba6b9e29')
	$Explorer.Location = 'Root'

	$Explorer.AsGetFiles = {
		New-FarFile -Name Flat -Description 'Flat explorer' -Attributes Directory
		New-FarFile -Name Tree -Description 'Tree explorer' -Attributes Directory
		New-FarFile -Name Path -Description 'Path explorer' -Attributes Directory
		New-FarFile -Name Location -Description 'Location explorer' -Attributes Directory
	}

	$Explorer.AsExploreDirectory = {
		param($Explorer, $_)
		switch($_.File.Name) {
			Flat { New-TestFlatExplorer }
			Tree { New-TestTreeExplorer 'HKCU:\Control Panel' }
			Path { New-TestPathExplorer $env:FARHOME\FarNet }
			Location { New-TestLocationExplorer $env:FARHOME\FarNet }
		}
	}

	$Explorer.AsCreatePanel = {
		param($Explorer, $_)
		$Panel = [FarNet.Panel]::new($Explorer)
		$Panel.Title = 'Root'
		$Panel.ViewMode = 'Descriptions'
		$Panel.SortMode = 'Unsorted'
		$Panel
	}

	$Explorer
}

# Flat data explorer. It is designed to show just one panel with some files,
# that is why it does not have the AsExploreX scripts.
# *) It is used to edit, view, and [CtrlQ] by AsGetContent and AsSetText.
# *) Editors/viewers are not modal and work even when the panel has closed.
function global:New-TestFlatExplorer {
	$Explorer = [PowerShellFar.PowerExplorer]::new('0024d0b7-c96d-443b-881a-d7f221182386')
	$Explorer.Functions = 'DeleteFiles, GetContent, SetText'
	$Explorer.Location = 'Flat'

	# Files are PowerShell functions
	$Explorer.AsGetFiles = {
		param($Explorer, $_)
		Get-ChildItem Function: | %{ New-FarFile -Name $_.Name -Description $_.Definition -Data $_ }
	}

	# Deletes selected functions
	$Explorer.AsDeleteFiles = {
		param($Explorer, $_)
		$_.Files | Remove-Item -LiteralPath { "Function:\$($_.Name)" }
	}

	# To edit, view and [CtrlQ] the function definition
	$Explorer.AsGetContent = {
		param($Explorer, $_)
		$_.CanSet = $Explorer.AsSetText -ne $null # for testing
		$_.UseText = $_.File.Data.Definition
		$_.UseFileExtension = '.ps1'
	}

	# Updates the function when it is edited
	$Explorer.AsSetText = {
		param($Explorer, $_)
		Set-Content "Function:\$($_.File.Name)" ($_.Text.TrimEnd())
	}

	# The panel
	$Explorer.AsCreatePanel = {
		param($Explorer)
		$Panel = [FarNet.Panel]::new($Explorer)
		$Panel.Title = 'Flat: Functions'
		$Panel
	}

	$Explorer
}

# Tree explorer. It navigates through the data tree where each node panel is a
# child of its parent panel. The core knows how to navigate to parents or to
# the root, the explorer does not have to worry.
# Navigation notes (compare with the Path explorer):
# *) [Ctrl\] navigates to the Root panel.
# *) [Esc] is the same as [CtrlPgUp]: it opens the parent panel.
function global:New-TestTreeExplorer($Path) {
	$Explorer = [PowerShellFar.PowerExplorer]::new('ed2e169e-852d-4934-8ec2-ec10fec11acd')
	$Explorer.Location = $Path

	# The files represent file system directories and files
	$Explorer.AsGetFiles = {
		param($Explorer, $_)
		Get-ChildItem $Explorer.Location | %{
			New-FarFile $_.PSChildName -Attributes 'Directory' -Description "$($_.Property)" -Data $_
		}
	}

	# Gets another explorer for the requested directory
	$Explorer.AsExploreDirectory = {
		param($Explorer, $_)
		$_.NewPanel = $true
		New-TestTreeExplorer $_.File.Data.PSPath
	}

	# The panel
	$Explorer.AsCreatePanel = {
		param($Explorer, $_)
		$Panel = [FarNet.Panel]::new($Explorer)
		$Panel.Title = "Tree: $($Explorer.Location)"
		$Panel
	}

	$Explorer
}

# Path explorer. It navigates through the data tree using paths. Navigation
# includes root and parent steps.
# Navigation notes (compare with the Tree explorer):
# *) [Ctrl\] navigates to the drive root, not to the Root panel.
# *) [Esc] closes the Path panel and opens the parent Root panel.
function global:New-TestPathExplorer($Path) {
	$Explorer = [PowerShellFar.PowerExplorer]::new('fd00a7cc-5ec1-4279-b659-541bbb5b2a00')
	$Explorer.Functions = 'GetContent'
	$Explorer.Location = $Path

	# The files represent file system directories and files
	$Explorer.AsGetFiles = {
		param($Explorer)
		Get-ChildItem -LiteralPath $Explorer.Location | New-FarFile
	}

	# Gets another explorer for the requested directory
	$Explorer.AsExploreDirectory = {
		param($Explorer, $_)
		New-TestPathExplorer $_.File.Data.FullName
	}

	# Gets the root explorer
	$Explorer.AsExploreRoot = {
		param($Explorer, $_)
		New-TestPathExplorer ([IO.Path]::GetPathRoot($Explorer.Location))
	}

	# Gets the parent explorer or nothing
	$Explorer.AsExploreParent = {
		param($Explorer, $_)
		$path = [IO.Path]::GetDirectoryName($Explorer.Location)
		if ($path) {
			New-TestPathExplorer $path
		}
	}

	# To edit, view and [CtrlQ]
	$Explorer.AsGetContent = {
		param($Explorer, $_)
		$_.CanSet = $true
		$_.UseFileName = Join-Path $Explorer.Location $_.File.Name
	}

	# Updates the panel title when explorers change
	$Explorer.AsEnterPanel = {
		param($Explorer, $_)
		$_.Title = "Path: $($Explorer.Location)"
	}

	$Explorer
}

# Location explorer. It also navigates through the data tree using paths. But
# with the 'ExploreLocation' function it works with pure files with no data.
# Navigation notes are the same as for the "Path" example.
function global:New-TestLocationExplorer($Path) {
	$Explorer = [PowerShellFar.PowerExplorer]::new('594e5d2e-1f00-4f25-902d-9464cba1d4a2')
	$Explorer.Functions = 'ExploreLocation'
	$Explorer.Location = $Path

	# The files represent file system directories and files
	$Explorer.AsGetFiles = {
		param($Explorer, $_)
		Get-ChildItem -LiteralPath $Explorer.Location | %{
			[FarNet.SetFile]::new($_, $false)
		}
	}

	# Gets another explorer for the requested location
	$Explorer.AsExploreLocation = {
		param($Explorer, $_)
		$Path = if ($_.Location.Contains(':')) { $_.Location } else { Join-Path $Explorer.Location $_.Location }
		New-TestLocationExplorer $Path
	}

	# Gets the parent explorer or nothing
	$Explorer.AsExploreParent = {
		param($Explorer, $_)
		$path = [IO.Path]::GetDirectoryName($Explorer.Location)
		if ($path) {
			New-TestLocationExplorer $path
		}
	}

	# Gets the root explorer
	$Explorer.AsExploreRoot = {
		param($Explorer, $_)
		New-TestLocationExplorer ([IO.Path]::GetPathRoot($Explorer.Location))
	}

	# Updates the panel title when explorers change
	$Explorer.AsEnterPanel = {
		param($Explorer, $_)
		$_.Title = "Location: $($Explorer.Location)"
	}

	$Explorer
}

### Open the explorer panel
(New-TestRootExplorer).CreatePanel().Open()
