<#
.Synopsis
	Demo step file for tests.

.Description
	This is a step file example.

	Run step by step:
	ps: Invoke-FarStepper Test-Stepper..ps1 -Confirm

	A step file contains two parts: standard code and steps, script blocks and
	macros. A file is invoked as a normal script, it may or may not do some job
	itself (Part 1). Normally it returns steps (Part 2). Empty output is fine.

	Steps are invoked in different scopes and local variables of one step are
	not available for another. In order to share data between steps use global
	variables or the provided hashtable $Data.

	The script also shows how to run modal steps without blocking other steps.
#>

### Part 1. Optional code invoked before steps.
# Good place for checks, throw, assert, return.

Assert-Far ($Far.Window.Count -eq 2) 'Close Far Manager internal windows before this test.' 'Assert'

### Part 2. Steps, script blocks and macros.

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
	# find file
	Find-FarFile 'far.exe.config'
}

'Keys"CtrlA" -- open attributes dialog'

{
	# test: a dialog exists and there is a valid control in it
	Assert-Far @(
		$Far.Dialog -ne $null
		$Far.Dialog[2].Text -eq 'far.exe.config'
	)
}

'Keys"Esc" -- exit dialog'

{
	# test: the window (panels) and item ('far.exe.config')
	Assert-Far -Panels ((Get-FarFile).Name -eq 'far.exe.config')
}

### HOW TO: start a modal dialog
{{
	# this command starts a modal dialog, but the step sequence
	# is not stopped because the command is RETURNED (by {{..}})
	# and the stepper knows how to process this case correctly
	$text = Read-Host

	# NOTE This check is done after some other steps below!
	Assert-Far ($text -eq 'Another text')
}}

'Keys"S a m p l e Space t e x t" -- type some text'

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

'Keys"Enter" -- enter typed text'

### HOW TO: open a modal editor
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

'Keys"Esc n" -- exit editor, do not save'

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

'print("Modeless Editor") -- type some text'

{
	# test: editor text
	Assert-Far ($Far.Editor.GetText() -eq 'Modeless Editor')
}

{
	# go to panels
	$Far.Window.SetCurrentAt(1)
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

'Keys"Tab" -- go to another panel'

{
	# open yet another panel
	$Panel = New-Object PowerShellFar.ObjectPanel
	$Panel.AddObjects((Get-ChildItem))
	$Panel.Open()
}

'Keys"Tab" -- go back to start panel'

{
	# go to editor
	$Far.Window.SetCurrentAt(1)
}

{
	# test: current window is editor
	Assert-Far -Editor
}

'Keys"Esc n" -- exit editor, do not save'

{
	# test: current window is panel
	Assert-Far -Panels
}

'Keys"Esc" -- exit module panel'

'Keys"Tab Esc Tab" -- go to another panel, exit, go back'

{
	# restore original panel path and item
	$Far.Panel.CurrentDirectory = $Data.Path
	$Far.Panel.Redraw($Data.Index, 0)
}
