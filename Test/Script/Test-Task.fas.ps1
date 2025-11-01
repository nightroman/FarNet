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
	Assert-Far $__[0].Text -eq 'Modeless dialog'
}
keys Esc

### Input

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Async.Test')
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Modeless dialog'

	$__[1].Text = '_221107_0640'
	$__.Close()
}
job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq '_221107_0640'

	$__.Close()
}

### Error

job {
	$Far.InvokeCommand('fn: script=Script; unload=true; method=Script.Async.Test')
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Modeless dialog'

	$__.Close()
}
job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq 'Empty string!'

	$__.Close()
}
