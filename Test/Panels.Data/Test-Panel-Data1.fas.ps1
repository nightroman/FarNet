<#
.Synopsis
	DB 1. Begin!
#>

job {
	# make data and connect
	& "$env:FarNetCode\Samples\Tests\Initialize-Test.far.ps1"
}

### Test Panel-DBTable

job {
	Panel-DBTable
}
job {
	Assert-Far $__.Title -eq 'main Tables'
}

# go to TestCategories, open it
job {
	Find-FarFile TestCategories
}
keys Enter
job {
	Assert-Far -Dialog
	Assert-Far $__[2].Text -eq "SELECT * FROM [TestCategories]"
}
keys Enter
### Test Panel-DBData on TestCategories

job {
	Assert-Far ($__.Title -like '*TestCategories')
}

# check the 1st record Name, Description
job {
	Find-FarFile 1
	Assert-Far -FileDescription 'Task' -FileOwner 'Task remarks'
}

# add a new record
keys F7
job {
	# member panel?
	# exactly 3 items? fixed unwanted DataRow extras
	Assert-Far @(
		$__.Title -eq 'Members: DataRow'
		$__.GetFiles().Count -eq 3
	)
}
# go to Category, enter a new value
job { Find-FarFile Category }
macro 'Keys"CtrlEnter Esc"'
macro 'Keys"= T e s t Enter"'
job { Assert-Far -FileDescription 'Test' }

# exit and save the record, check
macro 'Keys"Esc Enter"'
job {
	Assert-Far -FileName '' -FileDescription 'Test'
}

# return to tables
keys Esc
job { Assert-Far -FileName 'TestCategories' }

# go to TestNotes, open it
job {
	Find-FarFile TestNotes
}
keys Enter
job {
	Assert-Far -Dialog
	Assert-Far $__[2].Text -eq "SELECT * FROM [TestNotes]"
}
keys Enter
### Test Panel-DBData on TestNotes

job {
	Assert-Far ($__.Title -like '*TestNotes')
}

# check the 1st record Name, Description
job {
	Find-FarFile 1
	Assert-Far -FileDescription '1' -FileOwner 'Try to modify, insert and delete records.'
}

# delete this record
macro 'Keys"Del Enter"'
job { Assert-Far ($__.CurrentFile.Name -ne '1') }

# go to 2, enter, go to Note, set new value
job { Find-FarFile 2 }
keys Enter
job {
	# fixed unwanted extras and order
	$files = $__.GetFiles()
	Assert-Far @(
		$files.Count -eq 4
		$files[0].Name -eq 'NoteId'
		$files[1].Name -eq 'CategoryId'
	)
}
job { Find-FarFile Note }
macro 'Keys"CtrlEnter Esc"'
macro 'Keys"= N o t e 1 Enter"'
job { Assert-Far -FileDescription 'Note1' }

# exit, save, check
macro 'Keys"Esc Enter"'
job {
	Assert-Far -FileName '2' -FileDescription '2' -FileOwner 'Note1'
}

# exit to tables
keys Esc
job { Assert-Far -FileName 'TestNotes' }

# exit
keys Esc
