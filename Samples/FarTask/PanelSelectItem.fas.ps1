<#
.Synopsis
	How to use a panel task for selecting an item.

.Description
	This script runs the following loop:
	(1) show sample files in the object panel (try [CtrlPgDn] file properties)
	(2) when the panel is closed, if there is no current file (dots) then stop
	(3) open the selected file in the non-modal editor
	(4) when the editor is closed, repeat
#>

for() {
	fun {
		# make a panel with some data
		$panel = Get-ChildItem -LiteralPath $PSScriptRoot -File | Out-FarPanel -Return -Title 'Go to a file or dots and close.'

		# on closing, get the current file
		$panel.add_Closing({
			$Data.CurrentFile = $this.CurrentFile
		})

		# start and return panel task
		[FarNet.Tasks]::Panel($panel)
	}

	# check the result file
	if (!$Data.CurrentFile) {
		break
	}

	# edit the file
	fun {
		$editor = New-FarEditor "$PSScriptRoot\$($Data.CurrentFile)" -DisableHistory
		[FarNet.Tasks]::Editor($editor)
	}
}
