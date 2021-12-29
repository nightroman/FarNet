<#
.Synopsis
	How to use `keys` and `macro` jobs in task scripts.

.Description
	It opens "Apply command", types "cls", tests results.
#>

# ensure panels
job {
	Assert-Far -Panels -Message 'Please run from panels.'
}

# open Apply Command dialog using `keys`
keys CtrlG

# the dialog is shown
job {
	Assert-Far -Dialog
}

# type the text
macro 'print"cls"'

# test the typed text
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text -eq 'cls'
}

# invoke the typed command
keys Enter
