
job {
	Open-FarEditor $PSScriptRoot\PowerShell\App.fsx -DisableHistory
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'l' -- load
'@
job {
	Assert-Far -EditorTitle 'F# Output'
	Assert-Far $Far.Editor[1].Text -eq 'seq [answer; 42]'
}
macro 'Keys [[Esc Esc]] -- exit output and script'
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'0 Del Esc' -- kill session
'@
