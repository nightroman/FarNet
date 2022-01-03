
$config = "$env:FarNetCode\FSharpFar\samples\fsx-sample\.fs.ini".Replace('\', '\\')
$script = "$env:FarNetCode\FSharpFar\samples\fsx-sample\App2.fsx".Replace('\', '\\')
macro "print 'fs: //open with=$config'; Keys'Enter' -- open empty session"
job {
	Assert-Far -Editor
}
macro @"
print [[#load "$script"]]; Keys'ShiftEnter' -- test
"@
job {
	Assert-Far -Dialog
}
macro 'Keys"M a y Enter"'
job {
	Assert-Far $Far.Editor[3].Text -eq 'Hello, May!'
}
macro "Keys '# q u i t ShiftEnter'"
