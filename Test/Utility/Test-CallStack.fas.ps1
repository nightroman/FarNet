<#
.Synopsis
	PromptForChoice dialog and nested prompt.

.Description
	It is called 'CallStack' historically. It still can be used to play with
	stack by Get-PSCallStack in the interactive on [Suspend].
#>

run {
	if ($global:Error) {throw 'Please remove errors.'}

	# call the test, it opens the Inquire prompt on error
	& "$env:FarNetCode\Samples\Tests\Test-CallStack-.ps1"
}

job {
	# error message (in inquire dialog)?
	Assert-Far -Dialog
	$Controls = @($Far.Dialog.Controls)
	Assert-Far ($Controls[1].Text -like 'Cannot remove variable Far *')
	Assert-Far ($Controls[-1].Text -match 'Help&\?')
}

# choose 'suspend'
keys s
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
keys h
job {
	Assert-Far -Panels

	# Check the error if any (V2). V3 CTP2 does not generate the error.
	if ($global:Error) {
		$r = $global:Error[0].ToString()
		Assert-Far (
			($r -eq 'Command execution stopped because the user selected the Halt option.') -or
			($r -eq 'The running command stopped because the user selected the Stop option.') # v4.0
		)
		$global:Error.RemoveAt(0)
	}
}
