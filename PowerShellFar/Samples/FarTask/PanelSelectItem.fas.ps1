<#
.Synopsis
	How to use a panel task for selecting an item.

.Description
	This script runs the following loop:
	(1) show sample files in the object panel (you can [Enter] file properties)
	(2) when the panel is closed, if there is no current file (dots) then stop
	(3) open the selected file in the non-modal editor
	(4) when the editor is closed, repeat
#>

for() {
	job {
		$panel = Get-ChildItem -LiteralPath $PSScriptRoot -File |
		Out-FarPanel -Return -Title 'Go to a file or dots and close.'
		$panel.add_Closing({
			$Data.CurrentFile = $this.CurrentFile
		})
		[FarNet.Tasks]::Panel($panel)
	}

	if (!$Data.CurrentFile) {
		break
	}

	job {
		$editor = New-FarEditor "$PSScriptRoot\$($Data.CurrentFile)" -DisableHistory
		[FarNet.Tasks]::Editor($editor)
	}
}
