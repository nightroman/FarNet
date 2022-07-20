
macro "print [[js:@ $PSScriptRoot\..\Samples\exception.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'JavaScriptFar'
	Assert-Far $Far.Dialog[1].Text -eq 'Oops'
	Assert-Far $Far.Dialog[2].Text -eq "    at C:\ROM\FarDev\Code\Modules\JavaScriptFar\Samples\exception.js:3:1 -> throw 'Oops'"
	$Far.Dialog.Close()
}
