<#
.Synopsis
	DB 2.
#>

### Test Test-Panel-DBNotes.far.ps1 (simple lookup)

# open it
job { & "$env:FarNetCode\Samples\Tests\Test-Panel-DBNotes.far.ps1" }
job { Assert-Far ($Far.Panel -is [PowerShellFar.DataPanel]) }

# go to Note1
job {
	Find-FarFile 'Note1'
	Assert-Far -FileDescription 'Warning'
}

### Fix: F3/CtrlQ: exclude noisy data members
keys F3
job {
	Assert-Far -Viewer
	Assert-Far ([IO.File]::ReadAllText($Far.Viewer.FileName).Trim() -match '^Note.+?\r\nCategory.+?\r\nCreated.+?\r\nNoteId.+?\r\nCategoryId : \d+$')
}
keys Esc
job {
	Assert-Far -Panels
}

### Fix: Excluded members must be propagated
keys Enter
job {
	Find-FarFile NoteId -ErrorAction 0
	Assert-Far @(
		"File is not found: 'NoteId'." -eq $global:Error[0]
		$Far.Panel.Value.NoteId -eq 2
	)
	$global:Error.RemoveAt(0)
}

### Fix: enter null for a value type property. _110326_150007
job {
	Find-FarFile 'Note'
	Assert-Far -FileDescription 'Note1'
}
# enter null
keys ShiftDel
job {
	Assert-Far -FileDescription '<null>'
}
# No
macro 'Keys"Esc Right Enter"'
job {
	Assert-Far -FileName Note1
}

### Enter record, go to Category
keys Enter
job {
	Find-FarFile 'Category'
	Assert-Far -FileDescription 'Warning'
}

# enter Category, i.e. lookup panel, check found record
keys Enter
job {
	Assert-Far -FileName 'Warning' -FileDescription 'Warning remarks'
}

# go to Test, enter to select this lookup value
job { Find-FarFile 'Test' }
keys Enter
job {
	Assert-Far -FileName 'Category' -FileDescription 'Test'
}

# exit and save the modified record
macro 'Keys"Esc Enter"'
job {
	Assert-Far -FileName 'Note1' -FileDescription 'Test'
}

# exit the panel
keys Esc
job { Assert-Far -Native }

### Test Test-Panel-DBNotes.far.ps1 (generic lookup)

# open it
job { & "$env:FarNetCode\Samples\Tests\Test-Panel-DBNotes.far.ps1" -GenericLookup }
job { Assert-Far ($Far.Panel -is [PowerShellFar.DataPanel]) }

# go to Note1
job {
	Find-FarFile 'Note1'
	Assert-Far -FileDescription 'Test'
}

# enter record, go to Category
keys Enter
job {
	Find-FarFile 'Category'
	Assert-Far -FileDescription 'Test'
}

# enter Category, i.e. lookup panel, check found record
keys Enter
job {
	Assert-Far -FileName 'Test' -FileDescription ''
}

# go to Task end enter on it, check returnded lookup data
job { Find-FarFile 'Task' }
keys Enter
job {
	Assert-Far -FileName 'Category' -FileDescription 'Task'
}

# save and exit, it has to be DataPanel
macro 'Keys"CtrlS Esc"'
job { Assert-Far ($Far.Panel -is [PowerShellFar.DataPanel]) }

# exit
keys Esc
