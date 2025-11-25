<#
.Synopsis
	Test panel basics
#>

### test .GoToPath and then Get-FarPath
job {
	$__.GoToPath("$env:FarNetCode\web.ps1")
	Assert-Far -FileName web.ps1

	$Far.Panel2.GoToPath('C:\TEMP\Missed-Missed-Missed')
	Assert-Far $Far.Panel2.CurrentDirectory -eq C:\TEMP
	Assert-Far -Passive -FileName ..
}
job {
	Assert-Far @(
		(Get-FarPath -Mirror) -eq 'C:\TEMP\web.ps1'
		(Get-FarPath -Mirror -Selected) -eq 'C:\TEMP\web.ps1'
	)
}

### test .GoToName (dumb, test, fail) and (existing, missed)
# assume C:\ROM\APS is active
# mind test order
job {
	# dumb, existing
	$__.GoToName('fArNeT.SlNx')
	Assert-Far -FileName FarNet.slnx

	# test, existing
	Assert-Far ($__.GoToName('WeB.Ps1', $false))
	Assert-Far -FileName web.ps1

	# dumb, missed
	$__.GoToName('missed-missed')
	Assert-Far -FileName web.ps1

	# test, missed
	Assert-Far (!$__.GoToName('missed-missed', $false))
	Assert-Far -FileName web.ps1

	# fail, existing
	Assert-Far ($__.GoToName('FarNet.slnx', $true))
	Assert-Far -FileName FarNet.slnx

	# fail, missed
	$failed = $false
	try {
		$__.GoToName('missed-missed', $true)
	}
	catch {
		$failed = $true
		$global:Error.RemoveAt(0)
	}
	Assert-Far $failed
	Assert-Far -FileName FarNet.slnx
}

### tests Alt+Letter because Find-FarFile is to use in other places
#! Alt+Letter: directory
keys AltZ o o Esc
job {
	Assert-Far -FileName Zoo
}
#! Alt+Letter: with *
keys Alt* F a * N e * s l n x Esc
job {
	Assert-Far -FileName FarNet.slnx
}

# and test this
job {
	& "$PSScriptRoot\Test-Panel-Abc.far.ps1"
}
