
<#
.SYNOPSIS
	Shows internal Far name of pressed keys.
	Author: Roman Kuzmin

.DESCRIPTION
	The dialog shows internal Far names of pressed keys. These names are used
	in native Far macros and in $Far.PostKeys(), $Far.PostMacro().

	Press [Esc] twice or use mouse to click the button [Cancel] to close the
	dialog with no action. Other buttons close the dialog as well and:

	[Get]
	-- Returns the key name.

	[Copy]
	-- Copies the key name to clipboard.

.NOTES
	There is a native way to get key names. Type these four keys:
		[Ctrl.] [Ctrl.] [<key>] [Esc]
	The last [Esc] cancels creation of the recorded empty macro.
#>

### create the dialog
$dialog = $Far.CreateDialog(-1, -1, 40, 8)
$dialog.HelpTopic = ''

$0 = $dialog.AddBox(3, 1, 36, 6, 'Key names')

$1 = $dialog.AddText(5, 2, -1, 'Press a key. Esc+Esc to exit.')

$2 = $dialog.AddEdit(5, 3, 34, '')

$3 = $dialog.AddText(3, 4, -1, '')
$3.Separator = 1

$4 = $dialog.AddButton(0, 5, 'Get')
$4.CenterGroup = $true

$6 = $dialog.AddButton(0, 5, 'Copy')
$6.CenterGroup = $true

$5 = $dialog.AddButton(0, 5, 'Cancel')
$5.CenterGroup = $true

### process pressed keys
$2.add_KeyPressed({&{
	if ($_.Code -ne [FarNet.KeyCode]::Esc -or $2.Text -ne 'Esc') {
		$_.Ignore = $true
		$2.Text = $Far.KeyToName($_.Code)
	}
}})

### copy to clipboard
$6.add_ButtonClicked({&{
	$Far.CopyToClipboard($2.Text)
}})

### return the key name
if ($dialog.Show() -and $dialog.Selected -eq $4) {
	$2.Text
}
