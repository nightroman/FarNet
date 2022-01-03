
$config = "$PSScriptRoot\Vanilla\Vanilla.fs.ini".Replace('\','\\')

macro "print 'fs: //open with=$config'; Keys'Enter' -- open empty session"

job {
	Assert-Far -EditorTitle 'F# Vanilla.fs.ini *_??????_??????.interactive.fsx'
}

macro @'
print '#r "FarNet/FarNet.dll"'; Keys'ShiftEnter' -- ref FarNet.dll
'@

job {
	Assert-Far ($Far.Editor[3].Text -like "--> Referenced '*\FarNet/FarNet.dll' (file may be locked by F# Interactive process)")
}

macro "Keys '# q u i t ShiftEnter'"
