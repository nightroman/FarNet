
job {
	# make data
	Import-Module FarNet.SQLite
	Open-SQLite
	Set-SQLite @'
CREATE TABLE [Test] ([It] INTEGER PRIMARY KEY, [Category] TEXT);
INSERT INTO Test (Category) VALUES ('Task');
INSERT INTO Test (Category) VALUES ('Warning');
INSERT INTO Test (Category) VALUES ('Information');
'@

	# open panel
	Panel-DBData -SelectCommand 'SELECT * FROM Test' `
	-CloseConnection -DbConnection $db.Connection -DbProviderFactory $db.Factory
}
job {
	Assert-Far ($__.GetFiles() -join ' ') -eq '1 2 3'
}

# sort
macro 'Keys"F1 s C a t e g o r y Enter"'
job {
	Assert-Far ($__.GetFiles() -join ' ') -eq '3 1 2'
}

# filter
macro @'
Keys"F1 f C a t e g o r y Space l i k e Space ' * n * ' Enter"
'@
job {
	Assert-Far ($__.GetFiles() -join ' ') -eq '3 2'
}

# new, to be filtered out
keys F7
job {
	Find-FarFile Category
}
macro 'Keys"Enter A 1 Enter Esc Enter"'
job {
	Assert-Far -FileDescription A1
	Assert-Far $__.GetFiles().Count -eq 3
}

# update
keys CtrlR
job {
	Assert-Far -FileDescription Warning
	Assert-Far $__.GetFiles().Count -eq 2
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
	Assert-Far $__.GetFiles().Count -eq 3
}

# update
keys CtrlR
job {
	Assert-Far -FileDescription Warning
	Assert-Far ($__.GetFiles() -join ' ') -eq '5 3 2'
}

# OK
keys Esc
