
macro "print [[js:@ $env:FarNetCode\JavaScriptFar\Samples\async\flow.js]] Keys'Enter'"
Start-Sleep -Milliseconds 100
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq dialog1

	$Far.Dialog[1].Text = 'result1'
	$Far.Dialog.Close()
}
Start-Sleep -Milliseconds 100
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq result1

	$Far.Dialog[1].Text = 'result2'
	$Far.Dialog.Close()
}
Start-Sleep -Milliseconds 100
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq result
	Assert-Far $Far.Dialog[1].Text -eq result2
	$Far.Dialog.Close()
}
