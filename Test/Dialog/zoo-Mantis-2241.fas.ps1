# FarNet 5.2.14 + PowerShellFar 5.2.6

macro 'Keys[[CtrlG CtrlDown]]'
job {
	# used to hang due to Far critical sections and not main PowerShell threads
	Assert-Far ($Far.Window.Kind -eq 'Menu')
}
keys Esc
job {
	Assert-Far -Dialog
}
keys Esc
