
job {
	$r = try {$Far.InvokeCommand('fn:')} catch {$_}
	Assert-Far ("$r".Contains("Missing required parameter 'script' or 'module'."))

	$r = try {$Far.InvokeCommand('fn: script=x; module=x')} catch {$_}
	Assert-Far ("$r".Contains("Parameters 'script' and 'module' cannot be used together."))

	$global:Error.Clear()
}

run {
	$env:SCRIPT_DEMO_COUNT = 0
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Message ;; name=John Doe; age=42')
}

job {
	Assert-Far $env:SCRIPT_DEMO_COUNT -eq "1"

	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq 'name: John Doe, age: 42'

	$__.Close()
}

job {
	#! with `name=""` it just returns
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Script.Message ;; name=""')
	Assert-Far $env:SCRIPT_DEMO_COUNT -eq "2"
}

run {
	Assert-Far -Panels
	$Far.InvokeCommand('fn: module=FarNet.Demo; method=FarNet.Demo.Script.Message ;; name=John Doe; age=42')
}

job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq 'name: John Doe, age: 42'

	$__.Close()
}
