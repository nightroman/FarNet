# case [Debug] / [Add-Debugger] / Quit / [Ignore]

try {
	run {
		& "$PSScriptRoot\abc.ps1"
		Set-AddDebuggerIO

		# this asserts
		$r = & $env:FarNetCode\Samples\Tests\Test-Assert-Far.far.ps1

		# but this is still called due to [Ignore]
		$Data.result = $r
	}

	keys d d ### Debug Add-Debugger

	job {
		Assert-Far (Test-Path variable:\_Debugger)
		Assert-Far -DialogTypeId ([PowerShellFar.Guids]::PSPromptDialog)

		$Far.Dialog[2].Text = 'quit'
		$Far.Dialog.Close()
	}

	job {
		Assert-Far -DialogTypeId ([PowerShellFar.Guids]::AssertDialog)
	}

	keys i ### Ignore

	job {
		Assert-Far "$($Data.result)" -eq '@{Name=John Doe; Age=-42}'
	}
}
finally {
	job {
		Assert-Far (!(Test-Path variable:\_Debugger))
		Remove-AddDebuggerIO
	}
}
