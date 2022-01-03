<#
.Synopsis
	Short text and relatively long button line.

.Description
	FarNet 5.0.55: buttons were shown out of the dialog box.
#>

run {
	$a = New-Object FarNet.MessageArgs
	$a.Text = 'Short text'
	$a.Caption = 'Short text'
	$a.Buttons = "Long button 1", "Long button 2"
	$a.Position = New-Object FarNet.Point 1, 1
	$null = $Far.Message($a)
}
job {
	Assert-Far -Dialog
	Assert-Far @(
		$r = $Far.Dialog.Rect
		$r.Top -eq 1
		$r.Left -eq 1
		$r.Right -eq 45
		$r.Bottom -eq 7
	)
}
macro 'Keys"Esc" -- exit dialog'
