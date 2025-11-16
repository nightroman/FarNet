<#
.Synopsis
	Prompt() with several fields
#>

job { $Psf.RunCommandConsole() }

run {
	. $PSScriptRoot\zoo.ps1

	# this call prompts for several mandatory parameters
	$r = TestManyMandatoryParameters
	Assert-Far @(
		$r.Count -eq 3
		$r.Name -ceq 'user'
		$r.Tags.Count -eq 1
		$r.Tags[0] -ceq 'tag1'
		$r.Password -is [System.Security.SecureString]
	)
}
job {
	# Name
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadLineDialog)
	Assert-Far $Far.UI.GetBufferLineText(-3) -eq 'cmdlet TestManyMandatoryParameters at command pipeline position 1'
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Supply values for the following parameters:'
	Assert-Far $__[0].Text -eq 'Name: '
}
keys u s e r Enter
job {
	# Tags[0]
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Name: user'
	Assert-Far $__[0].Text -eq 'Tags[0]: '
}
keys t a g 1 Enter
job {
	# Tags[1]
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Tags[0]: tag1'
	Assert-Far $__[0].Text -eq 'Tags[1]: '
}
keys Enter # enter empty
job {
	# Password
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Tags[1]:'
	Assert-Far $__[0].Text -eq 'Password: '
}
keys p a s s Enter
job {
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Password: *'
}

job {
	$Psf.StopCommandConsole()
	[FarNet.Tasks]::WaitForPanels(9)
}
