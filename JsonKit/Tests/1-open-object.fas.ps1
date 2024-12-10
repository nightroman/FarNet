
job {
	$Far.InvokeCommand('jk:open file=x-object.json')
}

job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Object'
}

### string
job {
	Find-FarFile string
	Assert-Far -FileDescription '"текст"'
}

### array
job {
	Find-FarFile array
	Assert-Far -FileDescription '["текст"]'
}
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.Title -eq 'array'
	Assert-Far $Far.Editor[0].Text -eq '['
	Assert-Far $Far.Editor[1].Text -eq '  "текст"'
	$Far.Editor.Close()
}

### object
job {
	Find-FarFile object
	Assert-Far -FileDescription '{"string":"текст"}'
}
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.Title -eq 'object'
	Assert-Far $Far.Editor[0].Text -eq '{'
	Assert-Far $Far.Editor[1].Text -eq '  "string": "текст"'
	$Far.Editor.Close()
}

job {
	$Far.Panel.Close()
}
