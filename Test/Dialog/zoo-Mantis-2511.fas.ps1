<#
.Synopsis
	http://bugs.farmanager.com/view.php?id=2511
#>

macro 'Keys"CtrlG" -- open dialog'
job {
	Assert-Far -Dialog
}
macro 'Keys"F11 2 1 $ F a r . D i a l o g" -- open command box, type $Far.Dialog'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'PowerShellFar'
}
macro 'Keys"Enter" -- invoke, *** USED TO HANG ***'
job {
	Assert-Far -Viewer
}
macro 'Keys"Esc" -- exit viewer'
job {
	Assert-Far -Dialog
}
macro 'Keys"Esc" -- exit dialog'
