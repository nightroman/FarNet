
job {
	$Far.Panel.GoToPath("$PSScriptRoot\x-object.json")
}

### select one array from cursor file
job {
	$Far.InvokeCommand('jk:open select=$.array')
}
job {
	Assert-Far -Plugin -FileName '"текст"'
	Assert-Far $Far.Panel.Title -eq 'Array $.array'
	$Far.Panel.Close()
}

### select one string from cursor file
job {
	$Far.InvokeCommand('jk:open select=$.string')
}
job {
	Assert-Far -Plugin -FileName '"текст"'
	Assert-Far $Far.Panel.Title -eq 'Array $'
	$Far.Panel.Close()
}

### select one object from cursor file
job {
	$Far.InvokeCommand('jk:open select=$.nest1')
}
job {
	Assert-Far -Plugin -FileName nest2 -FileDescription '{"id":1,"name":"Joe"}'
	Assert-Far $Far.Panel.Title -eq 'Object $.nest1'
	$Far.Panel.Close()
}

### select from json panel
job {
	$Far.InvokeCommand('jk:open file=x-object.json')
}
job {
	Assert-Far $Far.Panel.Title -eq 'Object $'
	$Far.InvokeCommand('jk:open select=$.nest1')
}
job {
	Assert-Far $Far.Panel.Title -eq 'Object $'
	Assert-Far $Far.Panel.Files[1].Name -eq nest2
}
keys CtrlS
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'There is no source file.'
	$Far.Dialog.Close()
}
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Files[1].Name -eq nest2
}
keys Esc
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Object $'
}
keys Esc
