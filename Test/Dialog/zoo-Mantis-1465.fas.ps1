<#
.Synopsis
	http://bugs.farmanager.com/view.php?id=1465
#>

### cmdline

macro 'Keys("Esc 1 2 3")'

### ShiftRight
job {
	$Far.CommandLine.Caret = 0
	$Far.CommandLine.SelectText(0, 3)
}
keys ShiftRight
job {
	# fixed
	Assert-Far $Far.CommandLine.ActiveText -eq '23'
}

### ShiftLeft
job {
	$Far.CommandLine.Caret = 4
	$Far.CommandLine.SelectText(0, 3)
}
keys ShiftLeft
job {
	# fixed
	Assert-Far $Far.CommandLine.ActiveText -eq '12'
}

macro 'Keys("Esc") -- clear cmdline'

### dialog

macro 'Keys("CtrlG 1 2 3")'

### ShiftRight
job {
	$Far.Line.Caret = 0
	$Far.Line.SelectText(0, 3)
}
keys ShiftRight
job {
	# fixed
	Assert-Far $Far.Line.ActiveText -eq '23'
}

### ShiftLeft
job {
	$Far.Line.Caret = 4
	$Far.Line.SelectText(0, 3)
}
keys ShiftLeft
job {
	# fixed
	Assert-Far $Far.Line.ActiveText -eq '12'
}

macro 'Keys("Esc") -- exit dialog'
