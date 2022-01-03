<#
.Synopsis
	TabExpansion in RO editor.

.Description
	Used to be a bad error message.
#>

Set-Content -Force $env:TEMP\_140316_051636.ps1 @'
$x1234
$x12
'@

(Get-Item $env:TEMP\_140316_051636.ps1).IsReadOnly = $true

job {
	Open-FarEditor $env:TEMP\_140316_051636.ps1
	$Far.Editor.GoTo(4, 1)
}
macro 'Keys"F11 2 7" -- complete $x12'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'System.InvalidOperationException'
	Assert-Far $Far.Dialog[1].Text -eq 'Editor is locked for changes. Unlock by [CtrlL].'
}
macro 'Keys"Esc Esc" -- exit dialog and editor'
job {
	Assert-Far -Panels
	Remove-Item $env:TEMP\_140316_051636.ps1 -Force
}
