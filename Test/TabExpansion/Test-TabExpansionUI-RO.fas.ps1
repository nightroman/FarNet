<#
.Synopsis
	TabExpansion in RO editor.

.Description
	Used to be a bad error message.
#>

Set-Content -Force $env:TEMP\_140316_051636.ps1 @'
$DebugPre
'@

(Get-Item $env:TEMP\_140316_051636.ps1).IsReadOnly = $true

job {
	Open-FarEditor $env:TEMP\_140316_051636.ps1
	$Far.Editor.GoTo(9, 0)
}
macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7" -- complete $x12
'@
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
