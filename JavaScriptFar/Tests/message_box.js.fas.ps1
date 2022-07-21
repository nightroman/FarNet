
macro "print [[js:@ $PSScriptRoot\..\Samples\message_box.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from JavaScript!'

	$Far.Dialog.Close()
}
