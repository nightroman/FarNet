
$Data.FileName = "$env:FarNetCode\JavaScriptFar\Samples\interop-psf.js"
job {
	Open-FarEditor $Data.FileName -DisableHistory
}
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.FileName -eq $Data.FileName
}
keys F5
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.Title -eq 'JavaScript result'
	Assert-Far $Far.Editor[0].Text -eq '{'
	Assert-Far ($Far.Editor[1].Text -like '  "*":*,')
}
keys Esc Esc
