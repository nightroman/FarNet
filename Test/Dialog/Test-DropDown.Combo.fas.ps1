
$Data.log = ''
run {
	# expose variables
	$Data.dialog = $ExecutionContext.SessionState.PSVariable

	# dialog
	$d = $Far.CreateDialog(-1, -1, 52, 4)
	$t = $d.AddText(1, 1, 50, '140322_150840')

	# combo with "no" items
	#! add a fake item or drop down is not working
	$e = $d.AddComboBox(1, -1, 50, '')
	$null = $e.Add('')

	# opening, fill the list
	$e.add_DropDownOpening({
		$Data.log += '+Opening'
		$e.Items.Clear()
		$null = $e.Add('item1') #_140324_121545 fixed corrupted memory
		$null = $e.Add('item2')
	})

	# closed
	$e.add_DropDownClosed({
		$Data.log += '+Closed'
		Assert-Far $e.Items.Count -eq 2
	})

	# do
	$null = $d.Show()
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq '140322_150840'
}
macro 'Keys"CtrlDown Down Enter" -- open combo, pick item2'
job {
	Assert-Far @(
		$Data.dialog.GetValue('e').Text -eq 'item2'
		$Data.log -ceq '+Opening+Closed'
	)
}
macro 'Keys"Esc" -- exit dialog'
