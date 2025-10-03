<#
.Synopsis
	PromptForChoice dialog and nested prompt.
	!! 2025-05-18-1148

.Description
	It is called 'CallStack' historically. It still can be used to play with
	stack by Get-PSCallStack in the interactive on [Suspend].
#>

run {
	Assert-Far -Title Ensure -NoError

	# call the test, it opens the Inquire prompt on error
	& "$env:FarNetCode\Samples\Tests\Test-CallStack.ps1"
}

job {
	# error message (in inquire dialog)?
	Assert-Far -Dialog
	$Controls = @($Far.Dialog.Controls)
	Assert-Far ($Controls[1].Text -like 'Cannot remove variable Far *')
	$box = $Controls[-3]
	Assert-Far $box.Items[-1].Text -eq '&? Help'
}

# choose 'suspend'
keys s Enter
job {
	# editor?
	Assert-Far -EditorFileName *.interactive.ps1
}

# exit editor
keys Esc
job {
	Assert-Far -Dialog
}

# Halt --> error will be caught; otherwise Write-Error may cause problems in testing
keys h Enter
job {
	Assert-Far -Panels

	Assert-Far $Error.Count -eq 1
	Assert-Far "$($Error[0])" -eq 'The running command stopped because the user selected the Stop option.'
	$Error.Clear()
}
