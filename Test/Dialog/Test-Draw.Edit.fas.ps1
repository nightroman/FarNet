<#
.Synopsis
	Draw events in a control.
#>

$Data.log = ''
run {
	# dialog
	$d = $Far.CreateDialog(-1, -1, 52, 4)
	$t = $d.AddText(1, 1, 50, '140322_160028')

	# edit box
	$e = $d.AddEdit(1, -1, 50, '')

	# drawing
	$e.add_Drawing({
		$Data.log += '1'
	})

	# coloring
	$e.add_Coloring({
		$Data.log += '2'
	})

	# drawn
	$e.add_Drawn({
		$Data.log += '3'
	})

	# do
	$null = $d.Show()
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 140322_160028
}
macro 'Keys"Esc" -- exit dialog'
job {
	#! $n changes with Far and FarNet
	$n = 1
	Assert-Far -Panels
	Assert-Far $Data.log -eq ('123' * $n)
}
