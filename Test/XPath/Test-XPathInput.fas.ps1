
Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

$Data.XPathInput = "$PSScriptRoot\Demo-XFile.xq"

### Test 1: *) from file; *) enter a number
run {
	$Data.Result = [FarNet.Tools.XPathInput]::ParseFile($Data.XPathInput)
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Input variable'
}
macro 'Keys"1 2 3 4 5 Enter"'
job {
	$expr = $Data.Result.Expression
	$vars = $Data.Result.Variables
	Assert-Far @(
		$expr.StartsWith('//File')
		$vars.input -eq 12345
		$vars.input -is [double]
		$vars.number2 -eq 3.1415
		$vars.string2 -eq "Text '2'"
	)
}

### Test 2: *) from string; *) enter a string
run {
	$Data.Result = [FarNet.Tools.XPathInput]::ParseText([IO.File]::ReadAllText($Data.XPathInput))
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Input variable'
}
macro 'Keys"q w e r t y Enter"'
job {
	$expr = $Data.Result.Expression
	$vars = $Data.Result.Variables
	Assert-Far @(
		$expr.StartsWith('//File')
		$vars.input -eq 'qwerty'
		$vars.input -is [string]
		$vars.number2 -eq 3.1415
		$vars.string2 -eq "Text '2'"
	)
}
