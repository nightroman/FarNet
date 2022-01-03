### test 1 .fs

job {
	Open-FarEditor $PSScriptRoot\Vanilla\Vanilla.fs -DisableHistory
}
macro 'Keys [[F11 3 c]] -- check'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'No errors'
}
macro 'Keys [[Esc Esc]] -- exit dialog and editor'
job {
	Assert-Far -Panels
}

### test 2 .fsx

job {
	Open-FarEditor $PSScriptRoot\Vanilla\Vanilla.fsx -DisableHistory
}
macro 'Keys [[F11 3 c]] -- check'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'No errors'
}
macro 'Keys [[Esc Esc]] -- exit dialog and editor'
