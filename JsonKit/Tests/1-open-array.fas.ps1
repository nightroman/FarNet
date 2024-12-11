
job {
	$Far.InvokeCommand('jk:open file=x-array.json')
}

job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Array $'
}

job {
	Find-FarFile '"текст"'
}

keys F4

job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.Title -eq '"текст"'
	Assert-Far $Far.Editor[0].Text -eq 'текст'
	$Far.Editor.Close()
}

job {
	$Far.Panel.Close()
}
