<#
.Synopsis
	Prompt() with several fields
#>

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
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'cmdlet TestManyMandatoryParameters at command pipeline position 1'
	Assert-Far $Far.Dialog[1].Text -eq 'Supply values for the following parameters:'
	Assert-Far $Far.Dialog[2].Text -eq 'Name'
}
keys u s e r Enter
job {
	# Tags[0]
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'cmdlet TestManyMandatoryParameters at command pipeline position 1'
	Assert-Far $Far.Dialog[1].Text -eq 'Supply values for the following parameters:'
	Assert-Far $Far.Dialog[2].Text -eq 'Tags[0]'
}
keys t a g 1 Enter
job {
	# Tags[1]
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text -eq 'Tags[1]'
}
keys Enter # enter empty
job {
	# Password
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text -eq 'Password'
	Assert-Far $Far.Dialog[3].IsPassword
}
keys p a s s Enter
