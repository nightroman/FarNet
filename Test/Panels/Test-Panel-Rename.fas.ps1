<#
.Synopsis
	Test: rename file.

.Description
	Open Function:\ and rename one. It used to not set the new name current.
#>

# open panel, find A:
job {
	go Function:\
}
job {
	Find-FarFile A:
}

# start rename
keys ShiftF6
job {
	# the dialog
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Rename'
}

# enter zz:
macro 'Keys"z z : Enter"'
job {
	# renamed and current
	Assert-Far -Panels -FileName 'zz:'
}

# rename back to A:
macro 'Keys"ShiftF6 A : Enter"'
job {
	# renamed and current
	Assert-Far -Panels -FileName 'A:'
}

# end
keys Esc
