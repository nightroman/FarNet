
$config = "$PSScriptRoot\Vanilla\Vanilla.fs.ini".Replace('\','\\')
macro "print 'fs: //open with=$config'; Keys'Enter' -- open empty session"
job {
	Assert-Far -EditorTitle 'F# Vanilla.fs.ini *_??????_??????.interactive.fsx'
}
macro @'
Keys 'CtrlA Del'
print '//bar'
Keys 'ShiftEnter'
'@
job {
	# used to be the exception dialog, changed to written error
	Assert-Far -Editor
	Assert-Far $Far.Editor[1].Text -eq '(*('
	Assert-Far $Far.Editor[2].Text -eq "F# command: Unknown command 'bar'."
}
macro @'
Keys 'CtrlA Del'
print '//bar'
Keys 'Enter'
print '//bar'
Keys 'ShiftEnter'
'@
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[3].Text -eq ')*)'
}
macro "Keys '# q u i t ShiftEnter'"
