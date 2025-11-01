# 230805_1220

job {
	#! Use as script. If the code is here then the case is not reproducible.
	& "$PSScriptRoot\Test-z_230805_1220_Closure.far.ps1"
}

job {
	Assert-Far "$($global:Error[0])" -eq 'You cannot call a method on a null-valued expression.'

	$file = $__.Files[1]
	Assert-Far $file.Name -eq $null
	Assert-Far $file.Description -eq '12345'

	$__.Close()
	$global:Error.Clear()
}
