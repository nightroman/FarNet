
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\message-box.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScript!'

	$Far.Dialog.Close()
}
