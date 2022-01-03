<#
.Synopsis
	Fixed Message(MessageArgs)

.Description
	Issues:
	- If Position is used without Buttons then it is ignored.
	- If Caption is not set then null reference is thrown.
	- In a custom dialog messages ampersands make hotkeys.
	- The case "no buttons" is rather not implemeted.
#>

run {
	$a = New-Object FarNet.MessageArgs
	$a.Text = 'with &Ampersand'
	$a.Position = New-Object FarNet.Point 2, 2
	$null = $Far.Message($a)
}
job {
	Assert-Far -Dialog
	Assert-Far @(
		$d = $Far.Dialog
		$r = $d.Rect

		$r.Top -eq 2
		$r.Left -eq 2
		$r.Right -eq 26
		$r.Bottom -eq 6
		$d[1].Text -ceq 'with &Ampersand'
	)
}
macro 'Keys"Esc" -- exit'
