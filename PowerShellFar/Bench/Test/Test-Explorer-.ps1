
<#
.SYNOPSIS
	Test scripted explorers with panels.
	Author: Roman Kuzmin

.DESCRIPTION
	PowerExplorer is the fully functional explorer designed for scripts. Interface
	methods have related script block properties. Each method calls its script.

	This script shows a complex tree with explorers depending on top nodes.
	Technical details are explained in comments.
#>

# Root explorer - complex data tree with different explorers. It works like a
# menu where each item opens a different explorer in its own child panel.
function New-TestRootExplorer
{
	New-Object PowerShellFar.PowerExplorer 'Root' -Property @{
		AsExplore = {
			New-FarFile -Name 'Flat' -Description 'Flat explorer' -Attributes 'Directory'
			New-FarFile -Name 'Tree' -Description 'Tree explorer' -Attributes 'Directory'
			New-FarFile -Name 'Path' -Description 'Path explorer' -Attributes 'Directory'
		}
		AsExploreFile = {
			switch($_.File.Name) {
				'Flat' { New-TestFlatExplorer }
				'Tree' { New-TestTreeExplorer $env:FARHOME }
				'Path' { New-TestPathExplorer $env:FARHOME }
			}
		}
	}
}

# Flat data explorer. It is designed to show just one panel with some files,
# that is why it does not have the AsExploreFile script. It uses the default
# panel, that is why it does not have the AsMakePanel script.
# *) It allows to edit, view, and [CtrlQ] by AsExportFile and AsImportFile.
# *) Editors/viewers are not modal and work even when the panel has closed.
function global:New-TestFlatExplorer
{
	New-Object PowerShellFar.PowerExplorer 'Tree' -Property @{
		# The files represent PowerShell functions
		AsExplore = {
			Get-ChildItem Function: | %{ New-FarFile -Name $_.Name -Description $_.Definition -Data $_ }
		}
		# Just sets the panel properties once
		AsSetupPanel = {
			$_.Panel.Title = 'Flat: Functions'
			$_.Panel.TempFileExtension = '.ps1'
		}
		# Allows to edit, view and [CtrlQ] the function definition
		AsExportFile = {
			Set-Content -LiteralPath $_.FileName $_.File.Data.Definition -Encoding Unicode
		}
		# Updates the function definition when it is edited and saved
		AsImportFile = {
			Set-Content "Function:\$($_.File.Name)" ([IO.File]::ReadAllText($_.FileName))
		}
		# Removes the functions
		AsDeleteFiles = {
			$_.Files | Remove-Item -LiteralPath { "Function:\$($_.Name)" }
		}
	}
}

# Tree explorer. It navigates through the data tree where each node panel is a
# child of its parent panel. The core knows how to navigate to parents or to
# the root, the explorer does not have to worry.
# Navigation notes (compare with the Path explorer):
# *) [Ctrl\] navigates to the Root panel.
# *) [Esc] is the same as [CtrlPgUp]: it opens the parent panel.
function global:New-TestTreeExplorer($Path)
{
	New-Object PowerShellFar.PowerExplorer $Path -Property @{
		# The files represent file system directories and files
		AsExplore = {
			Get-ChildItem -LiteralPath $this.Location | New-FarFile
		}
		# Gets another explorer for the requested directory
		AsExploreFile = {
			if ($_.File.IsDirectory) {
				New-TestTreeExplorer $_.File.Data.FullName
			}
		}
		# Just sets the panel title once
		AsSetupPanel = {
			$_.Panel.Title = "Tree: $($this.Location)"
		}
	}
}

# Path explorer. It navigates through the data tree using paths. Navigation
# includes root and parent steps. It creates the panel once and reuses it.
# Navigation notes (compare with the Tree explorer):
# *) [Ctrl\] navigates to the drive root, not to the Root panel.
# *) [Esc] closes the Path panel and opens the parent Root panel.
function global:New-TestPathExplorer($Path)
{
	New-Object PowerShellFar.PowerExplorer $Path -Property @{
		# The files represent file system directories and files
		AsExplore = {
			Get-ChildItem -LiteralPath $this.Location | New-FarFile
		}
		# Gets another explorer for the requested directory
		AsExploreFile = {
			if ($_.File.IsDirectory) {
				New-TestPathExplorer $_.File.Data.FullName
			}
		}
		# Gets the root explorer
		AsExploreRoot = {
			New-TestPathExplorer ([IO.Path]::GetPathRoot($this.Location))
		}
		# Gets the parent explorer or nothing
		AsExploreParent = {
			$path = [IO.Path]::GetDirectoryName($this.Location)
			if ($path) {
				New-TestPathExplorer $path
			}
		}
		# Creates the panel once and then reuses it
		AsMakePanel = {
			$panelTypeId = [guid]'fd00a7cc-5ec1-4279-b659-541bbb5b2a00'
			if ($_.Panel.TypeId -ne $panelTypeId) {
				New-Object FarNet.Panel -Property @{ TypeId = $panelTypeId }
			} else {
				$_.Panel
			}
		}
		# Updates the panel title when explorers change
		AsUpdatePanel = {
			$_.Panel.Title = "Path: $($this.Location)"
		}
	}
}

### Open the panel with the Root explorer
$Panel = New-Object FarNet.Panel
$Panel.Explorer = New-TestRootExplorer
$Panel.Title = 'Root'
$Panel.ViewMode = 'Descriptions'
$Panel.SortMode = 'Unsorted'
$Panel.PanelDirectory = '*'
$Panel.Open()
