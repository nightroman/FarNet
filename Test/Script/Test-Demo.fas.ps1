
run {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Demo.Message :: name=John Doe; age=42')
}

job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'name: John Doe, age: 42'

	$Far.Dialog.Close()
}
