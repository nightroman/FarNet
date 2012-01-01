
<#
.Synopsis
	Generates template code for the current dialog.
	Author: Roman Kuzmin

.Description
	How to use: when any dialog is opened press F11, select 'PowerShellFar',
	'Invoke input code', invoke the command Generate-Dialog-.ps1 (with path if
	it is not in the system paths).

	The default output file is GeneratedDialog-.ps1 in the local module folder.
	Normally it is ready to start code to show a dialog that looks like the
	original one. Of course, it is just a dummy template.
#>

param
(
	# Where generated code is saved.
	$OutputPath = "$($Psf.Manager.GetFolderPath(0, 1))\GeneratedDialog-.ps1"
)

Set-StrictMode -Version 2

# Get the current dialog
$dialog = $Far.Dialog
if (!$dialog) { $Far.Message("Run this from a dialog"); return }

# escape string
function Esc([string]$s) { $s.Replace("'", "''") }

# out $c.property if it is true
function OutTrue([string]$property) { if ($c.$property) { "$vc.$property = `$true" } }

.{
	$r = $dialog.Rect
	"# Create the dialog"
	"`$dialog = `$Far.CreateDialog(-1, -1, $($r.Width), $($r.Height))"
	"`$dialog.HelpTopic = '$($dialog.HelpTopic)'"

	# Get controls
	for($e = 0;; ++$e) {

		$c = $dialog[$e]
		if (!$c) { break }
		''
		$r = $c.Rect
		$vc = "`$$e"
		if ($c -is [FarNet.Forms.IListBox]) {
			$text = "'$(Esc $c.Title)'"
		}
		else {
			$text = "'$(Esc $c.Text)'"
		}

		if ($c -is [FarNet.Forms.IBox]) {
			"$vc = `$dialog.AddBox($($r.Left), $($r.Top), $($r.Right), $($r.Bottom), $text)"
			OutTrue LeftText
			OutTrue ShowAmpersand
			OutTrue Single
		}
		elseif ($c -is [FarNet.Forms.IButton]) {
			"$vc = `$dialog.AddButton($($r.Left), $($r.Top), $text)"
			OutTrue CenterGroup
			OutTrue NoBrackets
			OutTrue NoClose
			OutTrue ShowAmpersand
			if ($dialog.Default -eq $c) {
				"`$dialog.Default = $vc"
			}
		}
		elseif ($c -is [FarNet.Forms.ICheckBox]) {
			"$vc = `$dialog.AddCheckBox($($r.Left), $($r.Top), $text)"
			"$vc.Selected = $($c.Selected)"
			OutTrue CenterGroup
			OutTrue ShowAmpersand
			OutTrue ThreeState
		}
		elseif ($c -is [FarNet.Forms.IComboBox]) {
			"$vc = `$dialog.AddComboBox($($r.Left), $($r.Top), $($r.Right), $text)"
			OutTrue AutoAssignHotkeys
			OutTrue DropDownList
			OutTrue ExpandEnvironmentVariables
			OutTrue NoAmpersands
			OutTrue NoClose
			OutTrue NoFocus
			OutTrue ReadOnly
			OutTrue SelectLast
			OutTrue SelectOnEntry
			OutTrue WrapCursor
			"$vc.Selected = 1"
			"`$null = $vc.Add('Value 1')"
			"`$null = $vc.Add('Value 2')"
		}
		elseif ($c -is [FarNet.Forms.IEdit]) {
			if ($c.Fixed) {
				"$vc = `$dialog.AddEditFixed($($r.Left), $($r.Top), $($r.Right), $text)"
				if ($c.Mask) {
					"$vc.Mask = '$(Esc $c.Mask)'"
				}
			}
			elseif ($c.IsPassword) {
				"$vc = `$dialog.AddEditPassword($($r.Left), $($r.Top), $($r.Right), $text)"
			}
			else {
				"$vc = `$dialog.AddEdit($($r.Left), $($r.Top), $($r.Right), $text)"
			}
			if ($c.History) {
				"$vc.History = '$(Esc $c.History)'"
			}
			OutTrue Editor
			OutTrue ExpandEnvironmentVariables
			OutTrue ManualAddHistory
			OutTrue NoAutoComplete
			OutTrue ReadOnly
			OutTrue SelectOnEntry
			OutTrue UseLastHistory
		}
		elseif ($c -is [FarNet.Forms.IListBox]) {
			"$vc = `$dialog.AddListBox($($r.Left), $($r.Top), $($r.Right), $($r.Bottom), $text)"
			"$vc.Bottom = '$(Esc $c.Bottom)'"
			OutTrue AutoAssignHotkeys
			OutTrue NoAmpersands
			OutTrue NoBox
			OutTrue NoClose
			OutTrue NoFocus
			OutTrue SelectLast
			OutTrue WrapCursor
			"$vc.Selected = 1"
			"`$null = $vc.Add('Value1')"
			"`$null = $vc.Add('Value2')"
		}
		elseif ($c -is [FarNet.Forms.IRadioButton]) {
			"$vc = `$dialog.AddRadioButton($($r.Left), $($r.Top), $text)"
			"$vc.Selected = `$$($c.Selected)"
			OutTrue CenterGroup
			OutTrue Group
			OutTrue MoveSelect
			OutTrue ShowAmpersand
		}
		elseif ($c -is [FarNet.Forms.IText]) {
			if ($c.Vertical) {
				"$vc = `$dialog.AddVerticalText($($r.Left), $($r.Top), $($r.Right), $text)"
			}
			else {
				"$vc = `$dialog.AddText($($r.Left), $($r.Top), $($r.Right), $text)"
			}
			OutTrue BoxColor
			OutTrue Centered
			OutTrue CenterGroup
			OutTrue ShowAmpersand
			if ($c.Separator) {
				"$vc.Separator = $($c.Separator)"
			}
		}
		elseif ($c -is [FarNet.Forms.IUserControl]) {
			"$vc = `$dialog.AddUserControl($($r.Left), $($r.Top), $($r.Right), $($r.Bottom))"
		}
		else {
			continue
		}

		# other properties
		OutTrue Disabled
		OutTrue Hidden
	}

	''
	"# Show the dialog"
	"`$null = `$dialog.Show()"

} > $OutputPath

# open in editor
Open-FarEditor $OutputPath
