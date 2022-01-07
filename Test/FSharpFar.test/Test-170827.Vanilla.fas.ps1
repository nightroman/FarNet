### test 1 .fs

job {
	Open-FarEditor $PSScriptRoot\Vanilla\Vanilla.fs -DisableHistory
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'c' -- check
'@
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
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'c' -- check
'@
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'No errors'
}
macro 'Keys [[Esc Esc]] -- exit dialog and editor'
