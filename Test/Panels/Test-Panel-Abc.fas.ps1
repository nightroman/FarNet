<#
.Synopsis
	Test panel basics
#>

### test .GoToPath and then Get-FarPath
job {
	$__.GoToPath('C:\ROM\Aps\About.ps1')
	Assert-Far -FileName About.ps1

	$Far.Panel2.GoToPath('C:\TEMP\Missed-Missed-Missed')
	Assert-Far $Far.Panel2.CurrentDirectory -eq C:\TEMP
	Assert-Far -Passive -FileName ..
}
job {
	Assert-Far @(
		(Get-FarPath -Mirror) -eq 'C:\TEMP\About.ps1'
		(Get-FarPath -Mirror -Selected) -eq 'C:\TEMP\About.ps1'
	)
}

### test .GoToName (dumb, test, fail) and (existing, missed)
# assume C:\ROM\APS is active
# mind test order
job {
	# dumb, existing
	$__.GoToName('AbOuT-AnY.Ps1')
	Assert-Far -FileName About-Any.ps1

	# test, existing
	Assert-Far ($__.GoToName('AbOuT.Ps1', $false))
	Assert-Far -FileName About.ps1

	# dumb, missed
	$__.GoToName('missed-missed')
	Assert-Far -FileName About.ps1

	# test, missed
	Assert-Far (!$__.GoToName('missed-missed', $false))
	Assert-Far -FileName About.ps1

	# fail, existing
	Assert-Far ($__.GoToName('About-Any.ps1', $true))
	Assert-Far -FileName About-Any.ps1

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
	Assert-Far -FileName About-Any.ps1
}

### tests Alt+Letter because Find-FarFile is to use in other places
#! Alt+Letter: directory
macro 'Keys"AltU s e d Esc"'
job {
	Assert-Far -FileName Used
}
#! Alt+Letter: with *
macro 'Keys"Alt* A b o u t * A n * p s 1 Esc"'
job {
	Assert-Far -FileName About-Any.ps1
}
