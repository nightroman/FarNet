
macro "print [[js:@ $PSScriptRoot\..\Samples\error.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'ReferenceError: missing is not defined'
	Assert-Far $Far.Dialog[2].Text -eq '    at C:\ROM\FarDev\Code\Modules\JavaScriptFar\Samples\error.js:3:1 -> missing'
	$Far.Dialog.Close()
}
