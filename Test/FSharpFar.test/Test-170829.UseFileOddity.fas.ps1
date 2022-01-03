
job {
	Open-FarEditor $PSScriptRoot\UseFileOddity\App.fsx -DisableHistory
}
macro 'Keys [[F11 3 l]] -- load'
job {
	Assert-Far -EditorTitle 'F# Output'
	Assert-Far $Far.Editor[1].Text -eq '42'
}
macro 'Keys [[Esc]] -- exit output'
macro 'Keys [[F11 3 c]] -- check'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'F# errors'
}
macro 'Keys [[Esc Esc]] -- exit dialog and editor'
macro 'Keys [[F11 3 0 Del Esc]] -- kill session'
