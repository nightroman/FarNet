
job {
	# open empty session
	$Far.InvokeCommand("fs: open: with=$env:FarNetCode\FSharpFar\samples\fsx-sample\.fs.ini")
}

job {
	Assert-Far -Editor
}

macro @"
print [[#load "$("$env:FarNetCode\FSharpFar\samples\fsx-sample\App2.fsx".Replace('\', '\\'))"]]; Keys'ShiftEnter'
"@

job {
	Assert-Far -Dialog
}

macro 'Keys"M a y Enter"'

job {
	Assert-Far $Far.Editor[3].Text -eq 'Hello, May!'
}

macro 'Keys[[# q u i t ShiftEnter]]'
