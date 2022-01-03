
macro 'Keys"CtrlG d i r" -- Type dir in Apply command dialog'
job {
	$$ = $Far.Line
	Assert-Far @(
		$$.WindowKind -eq 'Dialog'
		$$.SelectedText -eq $null
		'Empty' -eq $$.SelectionSpan
		$$.Text -eq 'dir'
		$$.Caret -eq 3
		$$.Length -eq 3
		$$.Index -eq -1
		$$.ActiveText -eq 'dir'
	)
}
macro 'Keys"ShiftLeft" -- Select the last char'
job {
	$$ = $Far.Line
	Assert-Far @(
		$$.WindowKind -eq 'Dialog'
		$$.SelectedText -eq 'r'
		'1 from 2 to 3' -eq $$.SelectionSpan
		$$.Text -eq 'dir'
		$$.Caret -eq 2
		$$.Length -eq 3
		$$.Index -eq -1
		$$.ActiveText -eq 'r'
	)
}
keys Esc
job {
	Assert-Far -Panels
}
