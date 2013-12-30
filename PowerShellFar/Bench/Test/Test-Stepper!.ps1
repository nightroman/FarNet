
<#
.Synopsis
	Test unit for the Stepper.
	Author: Roman Kuzmin

.Description
	It is a step unit example. Steps are macros and script blocks returned by
	this script. This script should not be invoked directly. Either use the
	cmdlet Invoke-FarStepper or call the test Test-Stepper-.ps1.

	A step unit contains two parts: standard code and script blocks. A unit is
	invoked as a normal script, it may or may not do some job itself (Part 1).
	Normally it returns steps (Part 2). It is fine to return nothing.

	Steps may add extra steps by $Psf.Stepper.Go(). Extra steps are inserted
	into the queue after the current. This is useful when all steps are not
	known beforehand, e.g. actual steps may depend on a user choice.

	Steps are invoked in different scopes and local variables of one step are
	not available for another. In order to share data between steps use global
	variables or, even better, the automatic variable $Data (shortcut for
	$Psf.Stepper.Data).

	The script also shows how to enter modal UI and continue steps in it.
#>

### Part 1. Optional code to be invoked before steps.
# - Good place to check prerequisites and throw on errors.
# - It is also fine to return with no steps returned at all.

# check prerequisites and throw on errors
Assert-Far ($Far.Window.Count -eq 1) "Close Far Manager internal windows before this test." "Assert"

### Part 2. Returned steps: returned keys and script blocks.

{
	# keep some data, use the automatic variable $Data
	$Data.Path = $Far.Panel.CurrentDirectory
	$Data.Index = $Far.Panel.CurrentIndex
}

{
	# hide panels
	$Far.Panel2.IsVisible = $false
	$Far.Panel.IsVisible = $false
}

{
	# show panels
	$Far.Panel.IsVisible = $true
	$Far.Panel2.IsVisible = $true
}

{
	# go to Far Manager home directory
	$Far.Panel.GoToPath("$env:FARHOME\")
}

{
	# find
	Find-FarFile 'far.exe.config'
}

# open the attributes dialog
'Keys"CtrlA"'

{
	# test: a dialog exists and there is a valid control in it
	Assert-Far @(
		$Far.Dialog -ne $null
		$Far.Dialog[2].Text -eq 'far.exe.config'
	)
}

# exit the dialog
'Keys"Esc"'

{
	# test: the window (panels) and item ('far.exe.config')
	Assert-Far -Panels ((Get-FarFile).Name -eq 'far.exe.config')
}

# HOW TO: start a modal dialog
{{
	# this command starts a modal dialog, but the step sequence
	# is not stopped because the command is RETURNED (by {{..}})
	# and the stepper knows how to process this case correctly
	Read-Host
}}

# type some text
'Keys"S a m p l e Space t e x t"'

{
	# test: a dialog exists and there is a valid control in it
	Assert-Far @(
		$Far.Dialog -ne $null
		$Far.Dialog[1].Text -eq 'Sample text'
	)
}

{
	# set some text
	$Far.Dialog[1].Text = 'Another text'
}

{
	# test: editbox text
	Assert-Far ($Far.Dialog[1].Text -eq 'Another text')
}

# exit the dialog
'Keys"Esc"'

# HOW TO: open a modal editor
{{
	# this command starts a modal editor, but the step sequence
	# is not stopped because the command is RETURNED (by {{..}})
	# and the stepper knows how to process this case correctly
	Open-FarEditor 'Test' -Modal -DisableHistory
}}

{
	# insert some text (use SetText to test it)
	$Far.Editor.SetText('Modal Editor')
}

{
	# test: editor text
	Assert-Far ($Far.Editor.GetText() -eq 'Modal Editor')
}

# exit the editor, do not save
'Keys"Esc n"'

{
	# test: current window is panel
	Assert-Far -Panels
}

{
	# open modeless editor
	Open-FarEditor 'Test' -DisableHistory
}

{
	# test: current window is editor
	Assert-Far -Editor
}

# insert some text
'print("Modeless Editor")'

{
	# test: editor text
	Assert-Far ($Far.Editor.GetText() -eq 'Modeless Editor')
}

{
	# go to panels
	$Far.Window.SetCurrentAt(0)
}

{
	# test: current window is panel
	Assert-Far -Panels
}

{
	# open a user panel with some objects
	$Panel = New-Object PowerShellFar.ObjectPanel
	$Panel.AddObjects((Get-ChildItem))
	$Panel.Open()
}

{
	# optionally open one more panel (by returned extra steps)
	if ($Psf.Stepper.Ask) {
		$answer = Show-FarMessage 'Open another panel?' -Choices 'Yes', 'No', 'Fail'
		if ($answer -eq 2) { throw "This is a demo error." }
		$Data.AnotherPanel = $answer -eq 0
	}
	else {
		$Data.AnotherPanel = $true
	}
	if ($Data.AnotherPanel) {
		$Psf.Stepper.Go(@(
			'Keys"Tab"'
			{
				# this step was added dynamically to open yet another panel
				$Panel = New-Object PowerShellFar.ObjectPanel
				$Panel.AddObjects((Get-ChildItem))
				$Panel.Open()
			}
			'Keys"Tab"'
		))
	}
}

{
	# go to editor
	$Far.Window.SetCurrentAt(1)
}

{
	# test: current window is editor
	Assert-Far -Editor
}

# exit the editor, do not save
'Keys"Esc n"'

{
	# test: current window is panel
	Assert-Far -Panels
}

# exit the plugin panel
'Keys"Esc"'

{
	# close one more panel (by returned extra steps)
	if ($Data.AnotherPanel) {
		$Psf.Stepper.Go(@(
			'Keys"Tab"'
			'Keys"Esc Tab"'
		))
	}
}

{
	# restore original panel path and item
	$Far.Panel.CurrentDirectory = $Data.Path
	$Far.Panel.Redraw($Data.Index, 0)
}
