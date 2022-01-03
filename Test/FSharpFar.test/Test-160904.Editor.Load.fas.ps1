
job {
	# open test file, type code, do not save
	Open-FarEditor c:/tmp/tmp.fsx -CodePage 65001 -DisableHistory
	$Far.Editor.SetText('printfn "привет_160904_185241"')
}
macro "Keys'F11 3 L' -- load script"
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
