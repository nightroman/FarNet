<#
.Synopsis
	Non-modal dialog input demo.

.Notes
	$Data.Input = $null is only needed in the strict mode.
	But it is self-documenting and safe on Set-StrictMode.

	GetNewClosure is not needed in Closing, unlike in sync cases
	(170929_054555), because GetNewClosure happens to be called
	internally by `job`.
#>

# init task data
$Data.Input = $null

# input using non-modal dialog
# $Data.Input is set by Closing
job {
	$dialog = $Far.CreateDialog(-1, -1, 52, 4)
	$null = $dialog.AddText(1, 1, 50, '_201123_rz')
	$edit = $dialog.AddEdit(1, -1, 50, '')

	$dialog.add_Closed({
		if ($_.Control) {
			$Data.Input = $edit.Text
		}
	})

	[FarNet.Tasks]::Dialog($dialog)
}

# use input (how to use in PowerShell scripts like this)
if ($null -ne $Data.Input) {
	job {
		$Far.Message($Data.Input)
	}
}

# return input (how to use with Start-FarTask -AsTask)
$Data.Input
