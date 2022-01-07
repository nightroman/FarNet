<#
Partially solved in 1.10.5 - if we edit files in Far editor then all is fine.
But in this test we edit externally -- the session becomes out of date.
#>

### test 1
job {
	Remove-Item C:\tmp\[z].Cache -Force -Recurse
	$null = mkdir C:\tmp\z.Cache

	Set-Content C:\tmp\z.Cache\Z.fs.ini @'
[fsi]
Z.fs
'@

	Set-Content C:\tmp\z.Cache\Z.fs @'
module Z
let z1 = 42
'@

	Set-Content C:\tmp\z.Cache\Z.fsx @'
printfn "%d" Z.z1
'@
}
job {
	Open-FarEditor C:\tmp\z.Cache\Z.fsx -DisableHistory
}

### check
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'l' -- load
'@
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[1].Text -eq '42'
}
macro 'Keys [[Esc Esc]] -- exit editors'

### test 2
job {
	Set-Content C:\tmp\z.Cache\Z.fs @'
module Z
let z2 = 11
'@

	Set-Content C:\tmp\z.Cache\Z.fsx @'
printfn "%d" Z.z2
'@
}
job {
	Open-FarEditor C:\tmp\z.Cache\Z.fsx -DisableHistory
}

### check
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'l' -- load
'@
job {
	Assert-Far -Editor
	Assert-Far ($Far.Editor[2].Text -like "*namespace or type 'z2' is not defined*")
}

macro 'Keys [[Esc Esc]] -- exit editors'

### test 3
job {
	# open module to save it in Far
	Open-FarEditor C:\tmp\z.Cache\Z.fs -DisableHistory
}
macro 'Keys [[F2 Esc]] -- save and exit editor -> reset session'
job {
	# open script to run
	Open-FarEditor C:\tmp\z.Cache\Z.fsx -DisableHistory
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'l' -- load
'@
job {
	# now it works due to reset session
	Assert-Far -Editor
	Assert-Far $Far.Editor[1].Text -eq '11'
}
macro 'Keys [[Esc Esc]] -- exit editors'

### end
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'0 Del Esc' -- kill session
'@
job {
	Remove-Item C:\tmp\z.Cache -Force -Recurse
}
