
job {
	$r = try {$Far.InvokeCommand('fn:')} catch {$_}
	Assert-Far ("$r".Contains("Missing required parameter 'script' or 'module'."))

	$r = try {$Far.InvokeCommand('fn: script=x; module=x')} catch {$_}
	Assert-Far ("$r".Contains("Parameters 'script' and 'module' cannot be used together."))

	$global:Error.Clear()
}

function Test-Dialog {
	job {
		Assert-Far -Dialog
		Assert-Far $Far.Dialog[1].Text -eq 'name: John Doe, age: 42'

		$Far.Dialog.Close()
	}
}

run {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Demo.Message :: name=John Doe; age=42')
}

Test-Dialog

run {
	$Far.InvokeCommand('fn: module=FarNet.Demo; method=FarNet.Demo.DemoMethods.Message :: name=John Doe; age=42')
}

Test-Dialog
