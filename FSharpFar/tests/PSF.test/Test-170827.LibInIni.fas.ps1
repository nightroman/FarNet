
job {
	Open-FarEditor $PSScriptRoot\LibInIni\LibInIni.fsx -DisableHistory
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'c' -- check
'@
job {
	Assert-Far -Editor
	$Far.Editor.Close()
}
