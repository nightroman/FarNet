<#
	Regression: could not open a panel from an editor.
#>

job {
	Open-FarEditor z.z
}
job {
	Assert-Far -Editor
}
job {
	1 | Out-FarPanel
}
job {
	Assert-Far -Panels -Plugin
}
macro 'Keys"Esc F12 2 Esc" -- exit panel and editor'
