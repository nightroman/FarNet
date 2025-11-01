<#
.Synopsis
	Test $__.CurrentDirectory

.Description
	_090929_061740 Far 3.0.4284 has no unwanted message.
#>

job {
	Assert-Far -Title Ensure -NoError
	$__.CurrentDirectory = 'C:\TEMP'
}
job {
	Assert-Far -Panels
	Assert-Far $__.CurrentDirectory -eq 'C:\TEMP'
}
job {
	$__.CurrentDirectory = $env:FARHOME
}
job {
	Assert-Far -Panels
	Assert-Far $__.CurrentDirectory -eq $env:FARHOME
}
job {
	$e = try {$__.CurrentDirectory = 'C:\missing'} catch {$_}
	Assert-Far @(
		$e
		'Exception setting "CurrentDirectory": "Cannot set panel directory: C:\missing"' -ceq $e
		$__.CurrentDirectory -eq $env:FARHOME
	)
	$global:Error.Clear()
}
