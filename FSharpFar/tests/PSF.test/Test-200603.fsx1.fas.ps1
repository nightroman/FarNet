
$config = "$env:FarNetCode\FSharpFar\samples\fsx-sample\.fs.ini".Replace('\', '\\')
macro "print 'fs: //open with=$config'; Keys'Enter' -- open empty session"
job {
	Assert-Far -Editor
}
macro @'
print [[Module1.hello "May"]]; Keys'ShiftEnter' -- test
'@
job {
	Assert-Far $Far.Editor[2].Text -eq 'Hello, May!'
}
macro "Keys '# q u i t ShiftEnter'"
