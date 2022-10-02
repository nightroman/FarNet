
job {
	# open empty session
	$Far.InvokeCommand("fs: //open with=$env:FarNetCode\FSharpFar\samples\fsx-sample\.fs.ini")
}

job {
	Assert-Far -Editor
}

macro 'print [[Module1.hello "May"]]; Keys"ShiftEnter"'

job {
	Assert-Far $Far.Editor[2].Text -eq 'Hello, May!'
}

macro 'Keys[[# q u i t ShiftEnter]]'
