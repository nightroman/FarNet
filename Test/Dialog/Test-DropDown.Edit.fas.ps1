
$Data.log = ''
run {
	# expose variables
	$Data.dialog = $ExecutionContext.SessionState.PSVariable

	# dialog
	$d = $Far.CreateDialog(-1, -1, 52, 4)
	$t = $d.AddText(1, 1, 50, '140322_151023')

	# edit box with history
	$e = $d.AddEdit(1, -1, 50, '')
	$e.History = 'ApplyCmd'

	# opening, fill the list
	$e.add_DropDownOpening({
		$Data.log += '+Opening'
	})

	# closed
	$e.add_DropDownClosed({
		$Data.log += '+Closed'
	})

	# do
	$null = $d.Show()
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq '140322_151023'
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
