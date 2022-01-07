<#
.Synopsis
	http://bugs.farmanager.com/view.php?id=2511
#>

macro 'Keys"CtrlG" -- open dialog'
job {
	Assert-Far -Dialog
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"1" -- open command box
'@
job {
	Assert-Far -DialogTypeId 416ff960-9b6b-4f3f-8bda-0c9274c75e53
	$Far.Dialog[2].Text = '$Far.Dialog'
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
