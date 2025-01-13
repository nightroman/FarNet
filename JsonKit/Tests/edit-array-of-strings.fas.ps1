
job {
	$Far.InvokeCommand('jk:open file=x-object.json')
}

### bad array
job {
	Find-FarFile arrayOfInt
}
macro 'if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "28541849-D26B-4456-8CDD-E14A2DFE9EE1") then Keys"s" end'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'This is not array of strings.'
	$Far.Dialog.Close()
}

### good array
job {
	Find-FarFile array
}
macro 'if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "28541849-D26B-4456-8CDD-E14A2DFE9EE1") then Keys"s" end'
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor.GetText() -eq "текст"
}

### empty text
job {
	$Far.Editor.SetText('')
	$Far.Editor.Save()
	$Far.Editor.Close()
}
job {
	Assert-Far -Plugin -FileName array -FileDescription '[""]'
	Assert-Far $Far.Panel.Explorer.IsDirty() -eq $true
}

### not empty text
macro 'if Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "28541849-D26B-4456-8CDD-E14A2DFE9EE1") then Keys"s" end'
job {
	$Far.Editor.SetText("текст1`nтекст2`n")
	$Far.Editor.Save()
	$Far.Editor.Close()
}
job {
	Assert-Far -Plugin -FileName array -FileDescription '["текст1","текст2",""]'
}

### done
job {
	$Far.Panel.Close()
}
