# https://forum.farmanager.com/viewtopic.php?t=12755
# FarNet 5.8.3

run {
	$dialog = $Far.CreateDialog(-1, -1, 52, 3)
	$edit = $dialog.AddEdit(1, 1, 50, '')
	$dialog.add_Initialized({
		$edit.Text = 'my edit'
		$edit.Line.Caret = 3
	})
	$null = $dialog.Show()
}

job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'my edit'
	Assert-Far $__[0].Line.Caret -eq 3
	$__.Close()
}
