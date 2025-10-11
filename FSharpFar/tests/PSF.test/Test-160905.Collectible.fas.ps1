<#
	It fails when collectible=true
#>

job {
	# open test file, type code, do not save
	Open-FarEditor c:/tmp/tmp.fsx -CodePage 65001 -DisableHistory
	$Far.Editor.SetText(@'
let msg(any) = far.Message((sprintf "%A" any), "F#")
type Fruit = {name : string; count : int}
let x = {name = "banana"; count = 3}
msg x
'@)
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'L' -- load script
'@
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'F#'
}
macro "Keys'Esc' -- exit dialog"
job {
	$editor = $Far.Editor
	Assert-Far -Editor
	Assert-Far $editor[0].Text -eq '[Loading C:\tmp\tmp.fsx]'
}
macro "Keys'Esc Esc' -- exit two editors"
job {
	Remove-Item c:/tmp/tmp.fsx
}
