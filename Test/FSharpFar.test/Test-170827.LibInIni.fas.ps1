
job {
	Open-FarEditor $PSScriptRoot\LibInIni\LibInIni.fsx -DisableHistory
}
macro 'Keys [[F11 3 c]] -- check'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'No errors'
}
macro 'Keys [[Esc Esc]] -- exit dialog and editor'
