<#
.Synopsis
	Non-modal dialog input demo.

.Notes
	$Data.Input = $null is only needed in the strict mode.
	But it is self-documenting and safe on Set-StrictMode.
#>

# init task data
$Data.Input = $null

# input using non-modal dialog
# $Data.Input is set in Closed
fun {
	$dialog = $Far.CreateDialog(-1, -1, 52, 4)
	$dialog.KeepWindowTitle = $true
	$null = $dialog.AddText(1, 1, 50, '_201123_rz')
	$edit = $dialog.AddEdit(1, -1, 50, '')

	$dialog.add_Closed({
		if ($_.Control) {
			$Data.Input = $edit.Text
		}
	}.GetNewClosure())

	[FarNet.Tasks]::Dialog($dialog)
}

# use input in the same task script
if ($null -ne $Data.Input) {
	job {
		$Far.Message($Data.Input)
	}
}

# return input for other tasks, see NestedTasks.fas.ps1
$Data.Input
