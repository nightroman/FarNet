
$Data.log = ''
job {
	# expose variables
	$Data.dialog = $ExecutionContext.SessionState.PSVariable

	# dialog
	$d = $Far.CreateDialog(-1, -1, 52, 4)
	$t = $d.AddText(1, 1, 50, '_170930_060119')

	# edit box with history
	$e = $d.AddEdit(1, -1, 50, '')
	$e.History = 'ApplyCmd'

	#! fixed, was not called
	$d.add_Initialized({
		$_.Control.Text = 'Initialized!'
	})

	# opening, fill the list
	$e.add_DropDownOpening({
		$Data.log += '+Opening'
	})

	# closed
	$e.add_DropDownClosed({
		$Data.log += '+Closed'
	})

	# do
	$d.Open()
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Initialized!'
}
macro 'Keys"CtrlDown Enter" -- open combo, pick first'
job {
	$e = $Data.dialog.GetValue('e')
	Assert-Far @(
		$e.Text -and $e.Text -ne '' #! if it is '' in CtrlG, alas, remove this ''
		$Data.log -ceq '+Opening+Closed'
	)
}
macro 'Keys"Esc" -- exit dialog'
