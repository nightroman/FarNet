
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\extras\error-reference.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'ReferenceError: answer is not defined'
	Assert-Far $Far.Dialog[2].Text -eq "    at $env:FarNetCode\JavaScriptFar\Samples\extras\error-reference.js:3:8"
	$Far.Dialog.Close()
}

macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\extras\error-throw-error.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'Error: Oops'
	Assert-Far $Far.Dialog[2].Text -eq "    at $env:FarNetCode\JavaScriptFar\Samples\extras\error-throw-error.js:3:7"
	$Far.Dialog.Close()
}

macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\extras\error-throw-string.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'Oops'
	$Far.Dialog.Close()
}
