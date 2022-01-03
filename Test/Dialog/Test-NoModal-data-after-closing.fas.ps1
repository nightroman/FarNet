# FarNet 5.2.14 does not updates controls on closing. In the Closing event the
# data are valid (live). But after closing (e.g. in a posted job) the dialog
# data are the same as on creation, not changed.

job {
	# dialog
	$dialog = $Far.CreateDialog(-1, -1, 52, 3)
	$edit = $dialog.AddEdit(1, 1, 50, '')

	# expose data for test
	$Data.dialog = $dialog
	$Data.edit = $edit

	# open non modal dialog
	$dialog.Open()
}
job {
	Assert-Far -Dialog
}
macro 'Keys"b a r Enter" -- enter some text'
job {
	Assert-Far -Panels
	Assert-Far @(
		$Data.edit.Text -eq 'bar'
		$Data.dialog.Selected -eq $Data.edit
	)
}
