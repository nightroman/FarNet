
Remove-Item c:/tmp/missing.*
Set-Content c:/tmp/missing.fsx @'
#r "FarNet/FarNet.dll"
FarNet.Far.Api.Message "_160903_160456"
'@

### temp session

$config = "$PSScriptRoot\Vanilla\Vanilla.fs.ini".Replace('\','\\')
macro "print 'fs: //exec file = c:/tmp/missing.fsx; with = $config'; Keys'Enter' -- exec in temp session"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq '_160903_160456'
}
macro "Keys'Esc F11 3 0 Enter' -- open interactive"
job {
	Assert-Far -EditorTitle 'F# Vanilla.fs.ini *_??????_??????.interactive.fsx'
}
macro "Keys'Esc F11 3 0 Del Esc' -- exit all"

### main session

macro "print 'fs: //exec file = c:/tmp/missing.fsx'; Keys'Enter' -- exec in main session"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq '_160903_160456'
}
macro "Keys'Esc F11 3 0 Enter' -- open interactive"
job {
	Assert-Far -EditorTitle 'F# main.fs.ini *_??????_??????.interactive.fsx'
}
macro "Keys'Esc' -- exit interactive"
job {
	Remove-Item c:/tmp/missing.fsx
}
