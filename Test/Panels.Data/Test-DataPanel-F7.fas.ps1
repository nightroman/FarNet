
function Get-F7EscNo {
	# create
	keys F7
	job {
		# member panel with a new row
		Assert-Far -Panels
		Assert-Far $Far.Panel.Title -eq 'Members: DataRow'
	}
	# exit
	keys Esc
	job {
		# the Save dialog
		Assert-Far -Dialog
		Assert-Far $Far.Dialog[0].Text -eq 'Save'
	}
	# [No]
	keys 'Right Enter'
	job {
		# original panel
		Assert-Far -Panels
		Assert-Far $Far.Panel.Title -eq '_110330_175246'
	}
}

job {
	# open a memory database, create a table
	. Connect-SQLite-.ps1 -Path ':memory:'
	$command = $DbConnection.CreateCommand()
	$command.CommandText = @'
CREATE TABLE Test (It INTEGER PRIMARY KEY, Name TEXT);
'@
	$null = $command.ExecuteNonQuery()

	# open panel with the data table
	Panel-DbData-.ps1 -CloseConnection -TableName Test -Title _110330_175246
}
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq '_110330_175246'
}

### Add and cancel from the empty panel
Get-F7EscNo

### Add a new record
keys F7
job {
	Find-FarFile It
}
macro 'Keys"Enter 1 Enter"'
job {
	Find-FarFile Name
}
macro 'Keys"Enter N a m e 1 Enter"'
macro 'Keys"Esc Enter"'
job {
	Assert-Far -FileName 1 -FileDescription Name1
}

### Add and cancel from the current file
Get-F7EscNo
job {
	Assert-Far -FileName 1 -FileDescription Name1
}

### Exit
keys Esc
