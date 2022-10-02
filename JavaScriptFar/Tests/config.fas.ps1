
run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\config\StringifyEnhancements.js")
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq '{"foo":123,"bar":"baz"}'
	$Far.Dialog.Close()
}

job {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\config\TaskPromiseConversion.js :: milliseconds=50")
}
Start-Sleep -Milliseconds 100
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'done in 50 milliseconds'
	$Far.Dialog.Close()
}

run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\config\WebLoadingAndSearchPath.js")
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JavaScriptFar'
	$Far.Dialog.Close()
}
