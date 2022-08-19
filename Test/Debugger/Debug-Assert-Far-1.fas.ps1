# case [Debug] finds attached debugger

try {
	run {
		& "$PSScriptRoot\abc.ps1"
		Set-AddDebuggerIO

		Add-Debugger.ps1
		Assert-Far $false
		throw 'unexpected'
	}

	job {
		Assert-Far -DialogTypeId ([PowerShellFar.Guids]::AssertDialog)
	}

	keys d ### Debug

	job {
		Assert-Far -Dialog
		Assert-Far $Far.Dialog[1].Text -eq 'Step (h or ? for help)'

		$Far.Dialog[2].Text = 'continue'
		$Far.Dialog.Close()
	}

	job {
		Assert-Far -DialogTypeId ([PowerShellFar.Guids]::AssertDialog)
		$Far.Dialog.Close()
	}
}
finally {
	job {
		Remove-AddDebuggerIO
		Restore-Debugger
	}
}
