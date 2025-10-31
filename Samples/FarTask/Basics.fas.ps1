﻿<#
.Synopsis
	How to use script tasks for testing.
#>

job {
	if ($Far.Window.Count -ne 2) {throw 'Exit editors, viewers, dialogs.'}

	# keep some data, use the automatic variable $Data
	$Data.Path = $Far.Panel.CurrentDirectory
	$Data.Index = $Far.Panel.CurrentIndex
}

job {
	# hide passive panel
	$Far.Panel2.IsVisible = $false
}

job {
	# hide active panel
	$Far.Panel.IsVisible = $false
}

job {
	# show active panel
	$Far.Panel.IsVisible = $true
}

job {
	# show passive panel
	$Far.Panel2.IsVisible = $true
}

job {
	# go to Far Manager home directory
	$Far.Panel.GoToPath("$env:FARHOME\")
}

job {
	# find file
	Find-FarFile 'Far.exe.example.ini'
}

keys CtrlA # attributes dialog

job {
	# test: the dialog and its control
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text -eq 'Far.exe.example.ini'
}

keys Esc # exit dialog

job {
	# test: the window (panels) and item ('Far.exe.example.ini')
	Assert-Far -Panels
	Assert-Far -FileName Far.exe.example.ini
}

### HOW TO: start a modal dialog
run {
	# This command starts a modal dialog but the task code is not blocked
	# because of `run`. The task continues when the dialog is shown.
	$text = Read-Host

	# NOTE This check is called after next jobs below when the above dialog
	# exits. That is why the input text is known, the dialog is automated.
	Assert-Far $text -eq 'Another text'
}

keys S a m p l e Space t e x t # type some text

job {
	# test: the dialog and its control
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Sample text'
}

job {
	# set some text
	$Far.Dialog[1].Text = 'Another text'
}

job {
	# test: editbox text
	Assert-Far $Far.Dialog[1].Text -eq 'Another text'
}

keys Enter # enter typed text

### HOW TO: open a modal editor
run {
	# This command starts a modal editor but the task code is not blocked
	# because of `run`, same as for the modal dialog example.
	Open-FarEditor 'Test' -Modal -DisableHistory
}

job {
	# insert some text (use SetText to test it)
	$Far.Editor.SetText('Modal Editor')
	$Far.Editor.Redraw() #! for steps
}

job {
	# test: editor text
	Assert-Far $Far.Editor.GetText() -eq 'Modal Editor'
}

keys Esc n # exit editor, do not save

job {
	# test: current window
	Assert-Far -Panels
}

job {
	# open modeless editor
	Open-FarEditor Test -DisableHistory
}

job {
	# test: current window is editor
	Assert-Far -Editor
}

macro 'print("Modeless Editor")' # type some text

job {
	# test: editor text
	Assert-Far $Far.Editor.GetText() -eq 'Modeless Editor'
}

job {
	# switch to panels
	$Far.Window.SetCurrentAt(1)
}

job {
	# test: current window
	Assert-Far -Panels
}

job {
	# open a module panel
	$Panel = New-Object PowerShellFar.ObjectPanel
	$Panel.AddObjects((Get-ChildItem))
	$Panel.Open()
}

keys Tab # go to another panel

job {
	# open yet another panel
	$Panel = New-Object PowerShellFar.ObjectPanel
	$Panel.AddObjects((Get-ChildItem))
	$Panel.Open()
}

keys Tab # go back to start panel

job {
	# switch to editor
	$Far.Window.SetCurrentAt(1)
}

job {
	# test: current window
	Assert-Far -Editor
}

keys Esc n # exit editor, do not save

job {
	# test: current window
	Assert-Far -Panels
}

keys Esc # exit module panel

keys Tab Esc Tab # go to another panel, exit, go back

job {
	# restore original panel path and item
	$Far.Panel.CurrentDirectory = $Data.Path
	$Far.Panel.Redraw($Data.Index, 0)
}
