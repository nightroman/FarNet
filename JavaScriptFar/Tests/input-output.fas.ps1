
run {
	$Far.InvokeCommand("js:@ $env:FarNetCode\JavaScriptFar\Samples\input-output.js")
}
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
