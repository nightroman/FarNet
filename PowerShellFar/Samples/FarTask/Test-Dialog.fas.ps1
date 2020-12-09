<#
.Synopsis
	Tests for Test-Dialog.far.ps1.

.Description
	How to start test steps from UI: run the test dialog Test-Dialog.far.ps1,
	push the button [Test] and watch the steps one by one.

	How to start from the command line:
	ps: Start-FarTask Test-Dialog.fas.ps1 -Confirm

.Parameter TestOpened
		Used to call this test from Test-Dialog.far.ps1 by [Test].
		It provides the dialog local data for testing.
#>

param(
	$TestOpened
)

# open the test dialog if not yet or get its locals
if ($TestOpened) {
	foreach($_ in $TestOpened.GetEnumerator()) {$Data[$_.Name] = $_.Value}
}
else {
	run {
		# run the dialog
		Assert-Far ($Far.Window.Kind -ne 'Dialog') 'Do not run this from a dialog'
		. "$PSScriptRoot\Test-Dialog.far.ps1" -Locals $Data
	}
	job {
		# dialog?
		Assert-Far ($Data.dialog -is [FarNet.Forms.IDialog])
	}
}

### Edit (standard)

# go to edit 1 (by hotkey) and type
keys 'AltT Home ShiftEnd Del о к а'
job {
	Assert-Far @(
		# editbox is current
		$Data.dialog.Focused -eq $Data.e1
		# its text
		$Data.e1.Text -eq 'ока'
	)
}
job {
	# set and test text
	$Data.e1.Text = 'волга'
	Assert-Far ($Data.e1.Text -eq 'волга')
}
job {
	# set and test text selection
	$Data.e1.Line.SelectText(1, 4)
	Assert-Far ($Data.e1.Line.SelectedText -eq 'олг')
}
job {
	# disable and check
	$Data.e1.Disabled = $true
	Assert-Far ($Data.e1.Disabled)
}
job {
	# enable and check
	$Data.e1.Disabled = $false
	Assert-Far (!$Data.e1.Disabled)
}
job {
	# test text and selection
	Assert-Far @(
		$Data.e1.Text -eq 'волга'
		$Data.e1.Line.SelectedText -eq 'олг'
	)
}
job {
	# test IsTouched and flip
	Assert-Far ($Data.e1.IsTouched)
	$Data.e1.IsTouched = $false
}
job {
	# test IsTouched and flip
	Assert-Far (!$Data.e1.IsTouched)
	$Data.e1.IsTouched = $true
	Assert-Far ($Data.e1.IsTouched)
}

### CheckBox (standard)

job {
	# go to checkbox
	$Data.dialog.Focused = $Data.x1
	Assert-Far ($Data.dialog.Focused -eq $Data.x1)

	# keep its state
	$Data.Value = $Data.x1.Selected
}

# switch checkbox
keys 'Space'
job {
	# test new checkbox state, should be different
	Assert-Far ($Data.Value -ne $Data.x1.Selected)
}

### CheckBox (three state)

job {
	# go to threestate ckeckbox, set state to 0
	$Data.dialog.Focused = $Data.x2
	$Data.x2.Selected = 0
}
job {
	# test focus and state
	Assert-Far @(
		$Data.dialog.Focused -eq $Data.x2
		$Data.x2.Selected -eq 0
	)
}

# switch
keys 'Space'
job {
	# test state
	Assert-Far ($Data.x2.Selected -eq 1)
}

# switch
keys 'Space'
job {
	# test state
	Assert-Far ($Data.x2.Selected -eq 2)
}

### Edit (fixed)

# type 'Text12345', mask should deny '12345'
keys 'Tab Home ShiftEnd Del T e x t 1 2 3 4 5'
job {
	# test focus and text
	Assert-Far @(
		$Data.dialog.Focused -eq $Data.e2
		$Data.e2.Text -eq 'Text   '
	)
}

### Edit (password)

keys 'Tab Del W o r d'
job {
	# test focus and text
	Assert-Far @(
		$Data.dialog.Focused -eq $Data.e3
		$Data.e3.Text -eq 'Word'
	)
}

### RadioButton

job {
	# go to button 1, set selected
	$Data.dialog.Focused = $Data.r1
	$Data.r1.Selected = $true
	Assert-Far @(
		$Data.dialog.Focused -eq $Data.r1
		$Data.r1.Selected
		!$Data.r2.Selected
	)
}

# go to button 2 and select
keys 'Right Space'
job {
	Assert-Far @(
		$Data.dialog.Focused -eq $Data.r2
		!$Data.r1.Selected
		$Data.r2.Selected
	)
}

### ListBox

job {
	# listbox: set title and bottom
	$Data.lb.Title = 'Title1'
	$Data.lb.Bottom = 'Bottom1'
	Assert-Far @(
		$Data.lb.Title -eq 'Title1'
		$Data.lb.Bottom -eq 'Bottom1'
	)
}
job {
	# listbox: set only title
	$Data.lb.Title = 'Title2'
	Assert-Far @(
		$Data.lb.Title -eq 'Title2'
		$Data.lb.Bottom -eq 'Bottom1'
	)
}
job {
	# listbox: set only bottom
	$Data.lb.Bottom = 'Bottom2'
	Assert-Far @(
		$Data.lb.Title -eq 'Title2'
		$Data.lb.Bottom -eq 'Bottom2'
	)
}

### [List]: ComboBox (edit), ComboBox (list), ListBox

job {
	# test IsTouched and flip
	Assert-Far (!$Data.ce.IsTouched)
	$Data.ce.IsTouched = $true
}
job {
	# test IsTouched and flip
	Assert-Far ($Data.ce.IsTouched)
	$Data.ce.IsTouched = $false
	Assert-Far (!$Data.ce.IsTouched)
}
job {
	# go to [List] button
	$Data.dialog.Focused = $Data.list
	Assert-Far ($Data.dialog.Focused -eq $Data.list)
}

# push the button 1st time
keys 'Enter'
job {
	# test listbox data
	Assert-Far @(
		$Data.lb.Title -match '^Fast '
		$Data.lb.Bottom -match '^WS '
	)
}

# push the button 2nd time
keys 'Enter'
job {
	# test listbox data
	Assert-Far ($Data.lb.Title -match '^Slow ')
}

### Exit the dialog if we opened or show 'done'
if ($TestOpened) {
	job {
		$Far.Message('Dialog was tested!')
		$Data.test.Disabled = $true
	}
}
else {
	# exit
	keys 'Esc'
	job {
		# no dialog
		Assert-Far ($Far.Window.Kind -ne 'Dialog')
	}
}
