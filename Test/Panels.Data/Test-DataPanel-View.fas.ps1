
job {
	# make data
	Import-Module DB; Connect-DBSQLite ':memory:'
	$null = Set-DB @'
CREATE TABLE [Test] ([It] INTEGER PRIMARY KEY, [Category] TEXT);
INSERT INTO Test (Category) VALUES ('Task');
INSERT INTO Test (Category) VALUES ('Warning');
INSERT INTO Test (Category) VALUES ('Information');
'@
	# open panel
	Panel-DbData-.ps1 -CloseConnection -SelectCommand "SELECT * FROM Test" -DbConnection $Connect.Connection -DbProviderFactory $Connect.Factory
}
job {
	Assert-Far ($Far.Panel.GetFiles() -join ' ') -eq '1 2 3'
}

# sort
macro 'Keys"F1 s C a t e g o r y Enter"'
job {
	Assert-Far ($Far.Panel.GetFiles() -join ' ') -eq '3 1 2'
}

# filter
macro @'
Keys"F1 f C a t e g o r y Space l i k e Space ' * n * ' Enter"
'@
job {
	Assert-Far ($Far.Panel.GetFiles() -join ' ') -eq '3 2'
}

# new, to be filtered out
keys F7
job {
	Find-FarFile Category
}
macro 'Keys"Enter A 1 Enter Esc Enter"'
job {
	Assert-Far -FileDescription A1
	Assert-Far $Far.Panel.GetFiles().Count -eq 3
}

# update
keys CtrlR
job {
	Assert-Far -FileDescription Warning
	Assert-Far $Far.Panel.GetFiles().Count -eq 2
}

# new, to be shown
keys F7
job {
	# same name is current
	Assert-Far -FileName Category
}
macro 'Keys"Enter A N Enter Esc Enter"'
job {
	Assert-Far -FileDescription AN
	Assert-Far $Far.Panel.GetFiles().Count -eq 3
}

# update
keys CtrlR
job {
	Assert-Far -FileDescription Warning
	Assert-Far ($Far.Panel.GetFiles() -join ' ') -eq '5 3 2'
}

# OK
keys Esc
