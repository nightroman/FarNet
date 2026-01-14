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
		Assert-Far $__[1].Text -eq 'Step (h or ? for help)'

		$__[2].Text = 'continue'
		$__.Close()
	}

	[FarNet.Tasks]::WaitForDialog(999).Wait()

	job {
		Assert-Far -DialogTypeId ([PowerShellFar.Guids]::AssertDialog)
		$__.Close()
	}
}
finally {
	job {
		Remove-AddDebuggerIO
		Restore-Debugger
	}
}
