
job {
	# open test file, type code, do not save
	Open-FarEditor c:/tmp/tmp.fsx -CodePage 65001 -DisableHistory
	$Far.Editor.SetText('printfn "привет_160904_185241"')
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "65BD5625-769A-4253-8FDE-FFCC3F72489D")
Keys'L' -- load script
'@
job {
	$editor = $Far.Editor
	Assert-Far -Editor
	Assert-Far @(
		$editor[0].Text -ceq '[Loading C:\tmp\tmp.fsx]'
		$editor[1].Text -ceq 'привет_160904_185241'
	)
}
macro "Keys'Esc Esc' -- exit two editors"
job {
	Remove-Item c:/tmp/tmp.fsx
}
