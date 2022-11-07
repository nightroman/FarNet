<#
.Synopsis
	Async fn-script.
#>

### Cancel

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Async.Test')
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Modeless dialog'
}
keys Esc

### Input

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Async.Test')
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Modeless dialog'

	$Far.Dialog[1].Text = '_221107_0640'
	$Far.Dialog.Close()
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq '_221107_0640'

	$Far.Dialog.Close()
}

### Error

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Async.Test')
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Modeless dialog'

	$Far.Dialog.Close()
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Empty string!'

	$Far.Dialog.Close()
}
