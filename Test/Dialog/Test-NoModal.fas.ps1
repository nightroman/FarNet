
$Data.log = ''
job {
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
	Assert-Far $__[1].Text -eq 'Initialized!'
}
macro 'Keys"CtrlDown Enter" -- open combo, pick first'
job {
	#! if it is '' in CtrlG, alas, remove this '' as last in history and repeat
	Assert-Far $__[1].Text

	Assert-Far $Data.log -eq '+Opening+Closed'
}
macro 'Keys"Esc" -- exit dialog'
