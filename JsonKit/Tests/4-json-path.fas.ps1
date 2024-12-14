
### omit file, select using cursor file
job {
	$Far.Panel.GoToPath("$PSScriptRoot\x-object.json")
	$Far.InvokeCommand('jk:open select=$.nest1')
}
job {
	Assert-Far -Plugin -FileName '{"nest2":{"id":1,"name":"Joe"}}'
	Assert-Far $Far.Panel.Title -eq 'Array $'
}

### omit file, select using array panel
job {
	$Far.InvokeCommand('jk:open select=$[0].nest2')
}
job {
	Find-FarFile '{"id":1,"name":"Joe"}'
	Assert-Far $Far.Panel.Title -eq 'Array $'
}

keys CtrlS
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'There is no source file.'
}

keys Esc ShiftEsc
