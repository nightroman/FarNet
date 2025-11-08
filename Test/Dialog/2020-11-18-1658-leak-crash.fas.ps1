# Stop and Free were not called due to exception in Closing.
# 2020-11-18-1658 After fixing it crashed at first.

job {
	Assert-Far -Title Ensure -NoError

	$form = $Far.CreateDialog(-1, -1, 52, 3)
	$null = $form.AddText(1, 1, 50, 'throw-in-Closing')
	$form.add_Closing({
		throw 42
	})
	$form.Open()
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq throw-in-Closing
}
keys Esc #! close by Esc
job {
	Assert-Far -Dialog
	Assert-Far @(
		$__[0].Text -eq 'Error in Closing'
		$__[1].Text -eq '42'
		$global:Error.Count -ne 0
	)
	$__.Close()

	$global:Error.Clear()
}
