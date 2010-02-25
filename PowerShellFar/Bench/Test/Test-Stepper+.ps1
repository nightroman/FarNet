
<#
.SYNOPSIS
	Test unit for PowerShellFar.Stepper.
	Author: Roman Kuzmin

.DESCRIPTION
	It is a step unit example. Steps are key sequences and script blocks
	returned by this script. This script should not be invoked directly.
	Instead, use a stepper to invoke it, see Test-Stepper-.ps1

	For simplicity this script has Part 1 and 2. This scheme is optional: a
	script is invoked as normal, it may or may not do some job itself (Part 1).
	But it normally should return steps (Part 2). If it returns no steps - it's
	fine, but in this case it should return nothing at all.

	Steps may add extra steps by $Psf.Stepper.Go(). Extra steps are inserted
	into the queue after the current. This is useful when all steps are not
	known beforehand, e.g. actual steps may depend on a user choice.

	Note that you have to use global variables to share data between steps
	because steps are invoked in different scopes and local variables of
	processed steps do not exist.

	The script also shows how to start modal UI and continue steps in modal
	mode (dialogs started by keys or commands, modal editors and etc.).
#>

### Part 1. Optional code to be invoked before steps.
# - Good place to check prerequisites and throw, for example.
# - It is also OK to return at this moment with no steps at all.

# prerequisites
Assert-Far ($Far.Window.Count -eq 1) "Close Far Manager internal windows before this test." "Assert"

### Part 2. Returned steps: returned keys and script blocks.

{
	# keep some data, use global variables!
	$global:TestStepperPath = $Far.Panel.Path
	$global:TestStepperIndex = $Far.Panel.CurrentIndex
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

# HOW TO: start a modal dialog by keys
{
	# open the attributes dialog; it is modal, but the step sequence
	# is not stopped because keys are RETURNED BY THE CURRENT STEP
	# and the stepper knows how to process this case correctly
	Trace-Far "Attribute dialog..."
	'CtrlA'
}

{
	# test: a dialog exists and there is a valid control in it
	Assert-Far @(
		$Far.Dialog -ne $null
		$Far.Dialog.GetControl(2).Text -eq 'far.exe.config'
	)
}

# exit the dialog
'Esc'

{
	# test: the window (panels) and item ('far.exe.config')
	Assert-Far @(
		$Far.Window.Kind -eq 'Panels'
		(Get-FarFile).Name -eq 'far.exe.config'
	)
}

# HOW TO: start a modal dialog programmatically
{{
	# this command starts a modal dialog, but the step sequence
	# is not stopped because the command is RETURNED (by {{..}})
	# and the stepper knows how to process this case correctly
	Trace-Far "Read-Host dialog..."
	Read-Host
}}

# type some text
'S a m p l e Space t e x t'

{
	# test: a dialog exists and there is a valid control in it
	Assert-Far @(
		$Far.Dialog -ne $null
		$Far.Dialog.GetControl(1).Text -eq 'Sample text'
	)
}

{
	# set some text
	$Far.Dialog.GetControl(1).Text = 'Another text'
}

{
	# test: editbox text
	Assert-Far ($Far.Dialog.GetControl(1).Text -eq 'Another text')
}

# exit the dialog
'Esc'

# HOW TO: open a modal editor
{{
	# this command starts a modal editor, but the step sequence
	# is not stopped because the command is RETURNED (by {{..}})
	# and the stepper knows how to process this case correctly
	Trace-Far "Modal editor..."
	Start-FarEditor 'Test' -Modal -DisableHistory
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
'Esc n'

{
	# test: current window is panel
	Assert-Far ($Far.Window.Kind -eq 'Panels')
}

{
	# open modeless editor
	Start-FarEditor 'Test' -DisableHistory
}

{
	# test: current window is editor
	Assert-Far ($Far.Window.Kind -eq 'Editor')
}

{
	# insert some text (use PostText to test it)
	$Far.PostText('Modeless Editor')
}

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
	Assert-Far ($Far.Window.Kind -eq 'Panels')
}

{
	# open a user panel with some objects
	$p = New-Object PowerShellFar.UserPanel
	$p.AddObjects((Get-ChildItem))
	$p.Show()
}

{
	# optionally open one more panel (by returned extra steps)
	if ($Psf.Stepper.Ask) {
		$answer = Show-FarMessage 'Open another panel?' -Choices 'Yes', 'No', 'Fail'
		if ($answer -eq 2) { throw "This is a demo error." }
		$global:TestStepperAnotherPanel = $answer -eq 0
	}
	else {
		$global:TestStepperAnotherPanel = $true
	}
	if ($global:TestStepperAnotherPanel) {
		$Psf.Stepper.Go(@(
			'Tab'
			{
				# this step was added dynamically to open yet another panel
				$p = New-Object PowerShellFar.UserPanel
				$p.AddObjects((Get-ChildItem))
				$p.Show()
			}
			'Tab'
		))
	}
}

{
	# go to editor
	$Far.Window.SetCurrentAt(1)
}

{
	# test: current window is editor
	Assert-Far ($Far.Window.Kind -eq 'Editor')
}

# exit the editor, do not save
'Esc n'

{
	# test: current window is panel
	Assert-Far ($Far.Window.Kind -eq 'Panels')
}

# exit the plugin panel
'Esc'

{
	# close one more panel (by returned extra steps)
	if ($global:TestStepperAnotherPanel) {
		$Psf.Stepper.Go(@(
			'Tab'
			'Esc Tab'
		))
	}
}

{
	# restore original panel path and item
	$Far.Panel.Path = $global:TestStepperPath
	$Far.Panel.Redraw($global:TestStepperIndex, 0)
}

{
	# last step, do final jobs and cleaning
	Trace-Far "Cleaning..."
	Remove-Item Variable:\TestStepper*
}
