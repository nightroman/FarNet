<#
.Synopsis
	Test $Far.Panel.CurrentDirectory

.Description
	_090929_061740 Far 3.0.4284 has no unwanted message.
#>

job {
	Assert-Far -Title Ensure -NoError
	$Far.Panel.CurrentDirectory = 'C:\TEMP'
}
job {
	Assert-Far -Panels
	Assert-Far $Far.Panel.CurrentDirectory -eq 'C:\TEMP'
}
job {
	$Far.Panel.CurrentDirectory = $env:FARHOME
}
job {
	Assert-Far -Panels
	Assert-Far $Far.Panel.CurrentDirectory -eq $env:FARHOME
}
job {
	$e = try {$Far.Panel.CurrentDirectory = 'C:\missing'} catch {$_}
	Assert-Far @(
		$e
		'Exception setting "CurrentDirectory": "Cannot set panel directory: C:\missing"' -ceq $e
		$Far.Panel.CurrentDirectory -eq $env:FARHOME
	)
	$global:Error.Clear()
}
