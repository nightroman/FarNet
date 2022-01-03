
job {
	Open-FarEditor $PSScriptRoot\PowerShell\App.fsx -DisableHistory
}
macro 'Keys [[F11 3 l]] -- load'
job {
	Assert-Far -EditorTitle 'F# Output'
	Assert-Far $Far.Editor[1].Text -eq 'seq [answer; 42]'
}
macro 'Keys [[Esc Esc]] -- exit output and script'
macro 'Keys [[F11 3 0 Del Esc]] -- kill session'
