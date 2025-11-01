# Could not delete the file after panel tables then open one table.
#
# NB `AUTOINCREMENT` -> system table `sqlite_sequence`
# NB weird: t1 panel with no data: just column `It`

if (Test-Path $PSScriptRoot\z.sqlite) {
	Remove-Item $PSScriptRoot\z.sqlite
}

job {
	Import-Module FarNet.SQLite
	Open-SQLite $PSScriptRoot\z.sqlite
	Set-SQLite 'create table t1 (it integer primary key autoincrement, name, memo)'
	Close-SQLite
}

run {
	Panel-SQLite $PSScriptRoot\z.sqlite
}

job {
	Assert-Far -Dialog
	$__.Close()
}

job {
	Assert-Far -Panels
}

job {
	Find-FarFile t1
}

#! this caused file lock
keys Enter

job {
	Assert-Far -Dialog
	$__.Close()
}

job {
	Assert-Far -Plugin
	Assert-Far $__.Title -eq t1
}

# exit both panels
keys Esc Esc

#! could not remove
Remove-Item $PSScriptRoot\z.sqlite
