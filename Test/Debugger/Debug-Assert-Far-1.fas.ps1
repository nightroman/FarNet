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
		Assert-Far -Dialog
		Assert-Far $Far.Dialog[0].Text -eq Assert-Far
	}

	keys d ### Debug

	job {
		Assert-Far -Dialog
		Assert-Far $Far.Dialog[1].Text -eq 'Step (h or ? for help)'

		$Far.Dialog[2].Text = 'continue'
		$Far.Dialog.Close()
	}

	job {
		Assert-Far -Dialog
		Assert-Far $Far.Dialog[0].Text -eq Assert-Far
		$Far.Dialog.Close()
	}
}
finally {
	job {
		Remove-AddDebuggerIO
		Restore-Debugger
	}
}
