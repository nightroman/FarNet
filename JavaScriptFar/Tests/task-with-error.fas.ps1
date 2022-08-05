#! mind possible race, do not use stepper

# start
macro "print [[js:task:@ $env:FarNetCode\JavaScriptFar\Samples\task-with-error.js :: milliseconds=600]] Keys'Enter'"

# wait for it started
while([FarNet.User]::Data['_220723_1411_state'] -ne 'start') {
	Start-Sleep -Milliseconds 100
}

job {
	# it still works, so we are in panels
	Assert-Far -Panels
}

# wait for it ended
while([FarNet.User]::Data['_220723_1411_state'] -ne 'end') {
	Start-Sleep -Milliseconds 100
}

job {
	# it ends with an error dialog
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Error: OK'
	Assert-Far ($Far.Dialog[2].Text -match '^    at test \(.+?\\task-with-error\.js:\d+:\d+\) ->     throw Error')
	Assert-Far ($Far.Dialog[3].Text -match '^    at .+?\\task-with-error\.js:\d+:\d+$')

	$Far.Dialog.Close()
}
