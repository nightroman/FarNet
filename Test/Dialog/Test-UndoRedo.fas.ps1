
run {
	$null = Read-Host undo/redo
}

keys A
keys B
job {
	Assert-Far -Dialog
	Assert-Far $__[2].Text -eq AB
}

keys CtrlZ
job {
	Assert-Far $__[2].Text -eq A
}

keys CtrlY
job {
	Assert-Far $__[2].Text -eq AB
	$__.Close()
}
