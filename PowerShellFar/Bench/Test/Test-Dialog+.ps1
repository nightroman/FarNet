
<#
.Synopsis
	Test unit for Test-Dialog-.ps1.
	Author: Roman Kuzmin

.Description
	This is a super-macro to automate testing of a dialog Test-Dialog-.ps1.
	Note that local variables defined in Test-Dialog-.ps1 are all visible.

	How to start these steps from UI: run the test dialog Test-Dialog-.ps1,
	push the button [Test] and watch the steps one by one.

.Notes
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

	Assert-Far ($Far.Window.Kind -ne 'Dialog') "Do not run this from a dialog" "Test-Dialog+.ps1"

	{{
		# run the dialog
		& $global:TestDialogScript
	}}

	{
		# dialog?
		Assert-Far ($dialog -is [FarNet.Forms.IDialog])
	}
}

### Edit (standard)

# go to edit 1 (by hotkey) and type
'AltT Home ShiftEnd Del о к а'
{
	Assert-Far @(
		# editbox is current
		$dialog.Focused -eq $e1
		# its text
		$e1.Text -eq 'ока'
	)
}

{
	# set and test text
	$e1.Text = 'волга'
	Assert-Far ($e1.Text -eq 'волга')
}

{
	# set and test text selection
	$e1.Line.SelectText(1, 4)
	Assert-Far ($e1.Line.SelectedText -eq 'олг')
}

{
	# disable and check
	$e1.Disabled = $true
	Assert-Far ($e1.Disabled)
}

{
	# enable and check
	$e1.Disabled = $false
	Assert-Far (!$e1.Disabled)
}

{
	# test text and selection
	Assert-Far @(
		$e1.Text -eq 'волга'
		$e1.Line.SelectedText -eq 'олг'
	)
}

{
	# test IsTouched and flip
	Assert-Far ($e1.IsTouched)
	$e1.IsTouched = $false
}

{
	# test IsTouched and flip
	Assert-Far (!$e1.IsTouched)
	$e1.IsTouched = $true
	Assert-Far ($e1.IsTouched)
}

### CheckBox (standard)

{
	# go to checkbox
	$dialog.Focused = $x1
	Assert-Far ($dialog.Focused -eq $x1)

	# keep its state
	$global:TestDialogValue = $x1.Selected
}

# switch checkbox
'Space'

{
	# test new checkbox state, should be different
	Assert-Far ($global:TestDialogValue -ne $x1.Selected)
}

### CheckBox (three state)

{
	# go to threestate ckeckbox, set state to 0
	$dialog.Focused = $x2
	$x2.Selected = 0
}

{
	# test focus and state
	Assert-Far @(
		$dialog.Focused -eq $x2
		$x2.Selected -eq 0
	)
}

# switch
'Space'
{
	# test state
	Assert-Far ($x2.Selected -eq 1)
}

# switch
'Space'
{
	# test state
	Assert-Far ($x2.Selected -eq 2)
}

### Edit (fixed)

# type 'Text12345', mask should deny '12345'
'Tab Home ShiftEnd Del T e x t 1 2 3 4 5'

{
	# test focus and text
	Assert-Far @(
		$dialog.Focused -eq $e2
		$e2.Text -eq 'Text   '
	)
}

### Edit (password)

'Tab Del W o r d'

{
	# test focus and text
	Assert-Far @(
		$dialog.Focused -eq $e3
		$e3.Text -eq 'Word'
	)
}

### RadioButton

{
	# go to button 1, set selected
	$dialog.Focused = $r1
	$r1.Selected = $true
	Assert-Far @(
		$dialog.Focused -eq $r1
		$r1.Selected
		!$r2.Selected
	)
}

# go to button 2 and select
'Right Space'

{
	Assert-Far @(
		$dialog.Focused -eq $r2
		!$r1.Selected
		$r2.Selected
	)
}

### ListBox

{
	# listbox: set title and bottom
	$lb.Title = 'Title1'
	$lb.Bottom = 'Bottom1'
	Assert-Far @(
		$lb.Title -eq 'Title1'
		$lb.Bottom -eq 'Bottom1'
	)
}

{
	# listbox: set only title
	$lb.Title = 'Title2'
	Assert-Far @(
		$lb.Title -eq 'Title2'
		$lb.Bottom -eq 'Bottom1'
	)
}

{
	# listbox: set only bottom
	$lb.Bottom = 'Bottom2'
	Assert-Far @(
		$lb.Title -eq 'Title2'
		$lb.Bottom -eq 'Bottom2'
	)
}

### [List]: ComboBox (edit), ComboBox (list), ListBox

{
	# test IsTouched and flip
	Assert-Far (!$ce.IsTouched)
	$ce.IsTouched = $true
}

{
	# test IsTouched and flip
	Assert-Far ($ce.IsTouched)
	$ce.IsTouched = $false
	Assert-Far (!$ce.IsTouched)
}

{
	# go to [List] button
	$dialog.Focused = $list
	Assert-Far ($dialog.Focused -eq $list)
}

# push the button 1st time
'Enter'

{
	# test listbox data
	Assert-Far @(
		$lb.Title -match '^Fast '
		$lb.Bottom -match '^WS '
	)
}

# push the button 2nd time
'Enter'

{
	# test listbox data
	Assert-Far ($lb.Title -match '^Slow ')
}

### Exit the dialog if we have opened it
if (!$TestOpened) {

	# exit
	'Esc'

	{
		# no dialog
		Assert-Far ($Far.Window.Kind -ne 'Dialog')
	}
}

{
	# end
	Remove-Variable -Scope global TestDialog*
}
