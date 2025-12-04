
run { $Far.InvokeCommand(":js:@ $env:FarNetCode\JavaScriptFar\Samples\extras\error-reference.js") }
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'ReferenceError: answer is not defined'
	Assert-Far $Far.Dialog[2].Text -eq "    at $env:FarNetCode\JavaScriptFar\Samples\extras\error-reference.js:4:8 -> answer = 42"
	$Far.Dialog.Close()
}

run { $Far.InvokeCommand(":js:@ $env:FarNetCode\JavaScriptFar\Samples\extras\error-throw-error.js") }
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'Error: Oops'
	Assert-Far $Far.Dialog[2].Text -eq "    at $env:FarNetCode\JavaScriptFar\Samples\extras\error-throw-error.js:3:7 -> throw Error('Oops')"
	$Far.Dialog.Close()
}

run { $Far.InvokeCommand(":js:@ $env:FarNetCode\JavaScriptFar\Samples\extras\error-throw-string.js") }
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'Oops'
	$Far.Dialog.Close()
}
