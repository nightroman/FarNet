<#
.Synopsis
	Example command menu created by a script.
	Author: Roman Kuzmin

.Description
	This script shows a menu with commands depending on the area and state.
#>

### Create and show the menu
New-FarMenu 'More' -Show -AutoAssignHotkeys -ChangeConsoleTitle $(
	### Editor actions
	if ($Far.Window.Kind -eq 'Editor') {
		$editor = $Far.Editor
		$file = $editor.FileName

		### Commands for editor selection
		if ($editor.SelectionExists) {
			New-FarItem '&e. Escape \," with \' { Set-Selection-.ps1 -Replace '([\\"])', '\$1' }
			New-FarItem '&u. Unescape \\,\"' { Set-Selection-.ps1 -Replace '\\([\\"])', '$1' }
			New-FarItem '.. Remove end spaces in selection' { $editor.SelectedLines | Remove-EndSpace-.ps1 }
			New-FarItem '.. Remove empty lines in selection' { Remove-EmptyString-.ps1 $editor.SelectedLines }
			New-FarItem -IsSeparator
		}

		### Other editor commands
		New-FarItem '&s. Remove end spaces (all text)' { $editor.Lines | Remove-EndSpace-.ps1 }
		New-FarItem '&d. Remove double empty lines (all text)' { Remove-EmptyString-.ps1 $editor.Lines 2 }
		New-FarItem '&f. Invoke a file from editor' { Invoke-FromEditor.ps1 }
	}
	### Panel actions
	elseif ($Far.Window.Kind -eq 'Panels') {
		$SelectedItems = @(Get-FarItem -Selected)

		# Update synopsis descriptions (how to use module features).
		New-FarItem "&d. Sync synopsis descriptions" { Import-Module FarDescription; Sync-FarDescriptionSynopsis $Far.Panel.CurrentDirectory }

		# Job: Remove items (can be very time consuming, really good candidate for a job).
		if ($SelectedItems) {
			New-FarItem "&r. Job: Remove $($SelectedItems.Count) selected item(s)" { Job-RemoveItem-.ps1 $SelectedItems }
		}

		# Start BITS transfer job or just open a panel (i.e. on dots)
		if ($SelectedItems) {
			New-FarItem "&b. BITS: Transfer $($SelectedItems.Count) selected item(s)" { Panel-BitsTransfer-.ps1 -Auto }
		}
		else {
			New-FarItem '&b. BITS: Show jobs panel' { Panel-BitsTransfer-.ps1 }
		}

		# Panel available performance counter set (use it to find out counter names and paths for Get-Counter)
		New-FarItem '&p. Performance counter set' { Get-Counter -ListSet * | Out-FarPanel 'CounterSetName' -Title 'Performance counter set' }
	}

	# Other commands
	New-FarItem -IsSeparator

	# Clear session
	New-FarItem '&c. Clear session' { Show-FarMessage (Clear-Session | Format-List | Out-String) -LeftAligned }

	# View/edit settings on a panel (note that $Psf.Settings.Save() is not called automatically)
	New-FarItem 'Settings' { Open-FarPanel $Psf.Settings -Title 'PowerShellFar Settings' }

	# Show global variables
	New-FarItem 'Show global variables' { Get-Variable -Scope global | Sort-Object Name | Format-Table -Auto | Out-Host }

	# Show processes and properties of the selected one
	New-FarItem 'Show process properties' { Get-Process | Out-FarList -Text 'Name' | Format-List * | Out-Host }
)
