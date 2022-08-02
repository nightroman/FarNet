
macro "print [[js:@$env:FarNetCode\JavaScriptFar\Samples\events\connect.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Connected, open any editor.'
	$Far.Dialog.Close()
}

macro "print [[js:@$env:FarNetCode\JavaScriptFar\Samples\events\connect.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Already connected, open any editor.'
	$Far.Dialog.Close()
}

run {
	Open-FarEditor $env:FarNetCode\JavaScriptFar\Samples\events\connect.js
}

job {
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[1].Text -like 'Opened *.js')
	Assert-Far $Far.Dialog[2].Text -eq 'Run disconnect.js to disconnect.'

	$Far.Dialog.Close()

	Assert-Far -Editor
	$Far.Editor.Close()
}

macro "print [[js:@$env:FarNetCode\JavaScriptFar\Samples\events\disconnect.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Disconnected.'
	$Far.Dialog.Close()
}

macro "print [[js:@$env:FarNetCode\JavaScriptFar\Samples\events\disconnect.js]] Keys'Enter'"
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Not connected.'
	$Far.Dialog.Close()
}
