# How to use `keys` and `macro` jobs in task scripts.
# It opens Apply Command, types and invokes "cls".
# It checks for expected results at each step.

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

# type the text, using `macro` this time, for test sake
macro 'Keys"c l s"'

# test the typed text
job {
	Assert-Far -Dialog ($Far.Dialog[2].Text -eq 'cls')
}

# invoke the typed command
keys Enter
