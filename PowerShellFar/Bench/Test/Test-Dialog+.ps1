
<#
.SYNOPSIS
	Test unit for Test-Dialog-.ps1.
	Author: Roman Kuzmin

.DESCRIPTION
	This is a super-macro to automate testing of a dialog Test-Dialog-.ps1.
	Note that local variables defined in Test-Dialog-.ps1 are all visible.

	How to start these steps from UI: run the test dialog Test-Dialog-.ps1,
	push the button [Test] and watch the steps one by one.

.NOTES
	This unit is used by:
	1) Test-Stepper-.ps1 - all steps are returned in this case, including steps
	opening the dialog in the beginning and steps closing the dialog in the
	end.
	2) Test-Dialog-.ps1 - this unit is called with switch -TestOpened because
	the dialog to be tested is already opened. In this case steps for opening
	and closing the dialog are not returned.
#>

param
(
	[switch]
	# Tells to test the dialog already opened by Test-Dialog-.ps1 (for example when button [Test] is clicked in this dialog).
	$TestOpened
)

# this code is invoked before steps as usual code
$global:TestDialogScript = Join-Path (Split-Path $MyInvocation.MyCommand.Path) 'Test-Dialog-.ps1'
$global:TestDialogValue = $null

# open the test dialog if not yet
if (!$TestOpened) {

	if ($Far.Window.Kind -eq 'Dialog') { throw "Do not run this from a dialog" }

	{{
		# run the dialog
		& $global:TestDialogScript
	}}

	{
		# dialog?
		if ($dialog -isnot [FarNet.Forms.IDialog]) { throw }
	}
}

### Edit (standard)

# go to edit 1 (by hotkey) and type
'AltT Home ShiftEnd Del о к а'
{
	# edit?
	if ($dialog.Focused -ne $e1) { throw }
	# text?
	if ($e1.Text -ne 'ока') { throw }
}

{
	# set and test text
	$e1.Text = 'волга'
	if ($e1.Text -ne 'волга') { throw }
}

{
	# set and test text selection
	$e1.Line.Select(1, 4)
	if ($e1.Line.Selection.Text -ne 'олг') { throw }
}

{
	# disable and check
	$e1.Disabled = $true
	if (!$e1.Disabled) { throw }
}

{
	# enable and check
	$e1.Disabled = $false
	if ($e1.Disabled) { throw }
}

{
	# test text and selection
	if ($e1.Text -ne 'волга') { throw }
	if ($e1.Line.Selection.Text -ne 'олг') { throw }
}

### CheckBox (standard)

{
	# go to checkbox
	$dialog.Focused = $x1
	if ($dialog.Focused -ne $x1) { throw }

	# keep its state
	$global:TestDialogValue = $x1.Selected
}

# switch checkbox
'Space'

{
	# test new checkbox state, should be different
	if ($global:TestDialogValue -eq $x1.Selected) { throw }
}

### CheckBox (three state)

{
	# go to threestate ckeckbox, set state to 0
	$dialog.Focused = $x2
	$x2.Selected = 0
}

{
	# test focus and state
	if ($dialog.Focused -ne $x2) { throw }
	if ($x2.Selected -ne 0) { throw }
}

# switch
'Space'
{
	# test state
	if ($x2.Selected -ne 1) { throw }
}

# switch
'Space'
{
	# test state
	if ($x2.Selected -ne 2) { throw }
}

### Edit (fixed)

# type 'Text12345', mask should deny '12345'
'Tab Home ShiftEnd Del T e x t 1 2 3 4 5'

{
	# test focus and text
	if ($dialog.Focused -ne $e2) { throw }
	if ($e2.Text -ne 'Text   ') { throw }
}

### Edit (password)

'Tab Del W o r d'

{
	# test focus and text
	if ($dialog.Focused -ne $e3) { throw }
	if ($e3.Text -ne 'Word') { throw }
}

### RadioButton

{
	# go to button 1, set selected
	$dialog.Focused = $r1
	$r1.Selected = $true

	# test
	if ($dialog.Focused -ne $r1) { throw }
	if (!$r1.Selected) { throw }
	if ($r2.Selected) { throw }
}

# go to button 2 and select
'Right Space'

{
	# test
	if ($dialog.Focused -ne $r2) { throw }
	if ($r1.Selected) { throw }
	if (!$r2.Selected) { throw }
}

### ListBox

{
	# listbox: set title and bottom
	$lb.Title = 'Title1'
	$lb.Bottom = 'Bottom1'
	# test
	if ($lb.Title -ne 'Title1') { throw }
	if ($lb.Bottom -ne 'Bottom1') { throw }
}

{
	# listbox: set only title
	$lb.Title = 'Title2'
	# test
	if ($lb.Title -ne 'Title2') { throw }
	if ($lb.Bottom -ne 'Bottom1') { throw }
}

{
	# listbox: set only bottom
	$lb.Bottom = 'Bottom2'
	# test
	if ($lb.Title -ne 'Title2') { throw }
	if ($lb.Bottom -ne 'Bottom2') { throw }
}

### [List]: ComboBox (edit), ComboBox (list), ListBox

{
	# go to [List] button
	$dialog.Focused = $list
	if (!$dialog.Focused -eq $list) { throw }
}

# push the button 1st time
'Enter'

{
	# test listbox data
	if ($lb.Title -notmatch '^Fast ') { throw }
	if ($lb.Bottom -notmatch '^WS ') { throw }
}

# push the button 2nd time
'Enter'

{
	# test listbox data
	if ($lb.Title -notmatch '^Slow ') { throw }
}

### Exit the dialog if we have opened it
if (!$TestOpened) {

	# exit
	'Esc'

	{
		# no dialog?
		if ($Far.Window.Kind -eq 'Dialog') { throw }
	}
}

{
	# end
	Remove-Item Variable:\TestDialog*
}
