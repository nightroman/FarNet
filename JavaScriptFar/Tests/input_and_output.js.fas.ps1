
macro "print [[js:@ $PSScriptRoot\..\Samples\input_and_output.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Enter your name'
	Assert-Far $Far.Dialog[2].Text -eq 'John Doe'

	$Far.Dialog[2].Text = 'Foo Bar'
	$Far.Dialog.Close()
}
job {
	Assert-Far $Far.UI.GetBufferLineText(-2) -eq 'Hello, Foo Bar'
}
