﻿
function Get-F7EscNo {
	# create
	keys F7
	job {
		# member panel with a new row
		Assert-Far -Panels
		Assert-Far $__.Title -eq 'Members: DataRow'
	}
	# exit
	keys Esc
	job {
		# the Save dialog
		Assert-Far -Dialog
		Assert-Far $__[0].Text -eq 'Save'
	}
	# [No]
	keys 'Right Enter'
	job {
		# original panel
		Assert-Far -Panels
		Assert-Far $__.Title -eq '_110330_175246'
	}
}

job {
	# open a memory database, create a table
	Import-Module FarNet.SQLite
	Open-SQLite
	Set-SQLite 'CREATE TABLE Test (It INTEGER PRIMARY KEY, Name TEXT);'

	# open panel with the data table
	Panel-DBData -TableName Test -Title _110330_175246 `
	-CloseConnection -DbConnection $db.Connection -DbProviderFactory $db.Factory
}
job {
	Assert-Far -Plugin
	Assert-Far $__.Title -eq '_110330_175246'
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
