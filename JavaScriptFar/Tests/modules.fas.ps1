
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\modules.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScriptFar'

	$Far.Dialog.Close()
}
