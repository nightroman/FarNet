<#
.Synopsis
	Drop down used to break steps.
	Not a problem with task jobs.
#>

run {
	# show dialog with a combo
	$d = $Far.CreateDialog(-1, -1, 52, 3)
	$e = $d.AddComboBox(1, 1, 50, '')
	$null = $e.Add('item1')
	$null = $e.Add('item2')
	$null = $d.Show()
}

macro 'Keys"CtrlDown" -- open combo'

job {
	Assert-Far ($Far.Window.Kind -eq 'ComboBox')
}

keys 'Down Enter'

job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'item2'
}

keys Esc
