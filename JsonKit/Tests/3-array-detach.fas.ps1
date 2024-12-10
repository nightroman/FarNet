
job {
	$Far.InvokeCommand('jk:open file=x-array.json')
}

keys F4
job {
	Assert-Far -Editor
	$Editor = $Far.Editor
	Assert-Far $Editor.Line.Text -eq null

	$Editor.Line.Text = 11
	$Editor.Save()

	$Far.Window.SetCurrentAt(-1)
}
job {
	Assert-Far -Plugin -FileName 11
}

keys End Del Enter # remove the last item
job {
	$Far.Window.SetCurrentAt(1)
}
run {
	Assert-Far -Editor
	$Far.Editor.Line.Text = 22
	$Far.Editor.Save()
}

job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Cannot find the source.'
	$Far.Dialog.Close()
}
job {
	Assert-Far -Editor
	Start-Sleep 1 #! odd
	$Far.Editor.Close()
}

job {
	Assert-Far -Plugin
	Find-FarFile 11
	$Far.Panel.Close()
}
