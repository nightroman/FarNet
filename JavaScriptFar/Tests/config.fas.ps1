
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\config\StringifyEnhancements.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq '{"foo":123,"bar":"baz"}'
	$Far.Dialog.Close()
}

macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\config\WebLoadingAndSearchPath.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JavaScriptFar'
	$Far.Dialog.Close()
}
