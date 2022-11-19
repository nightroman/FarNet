<#
.Synopsis
	Initializes the test database.

.Description
	Requires the package FarNet.SQLite.
#>

[CmdletBinding()]
param()

Import-Module $env:FARHOME\FarNet\Lib\FarNet.SQLite

# close
$DbConnection = $PSCmdlet.GetVariableValue('DbConnection')
if ($DbConnection) {
	$DbConnection.Close()
}

# make temp database
$database = "$env:TEMP\TempDB.sqlite"
if (Test-Path -LiteralPath $database) {
	Remove-Item -LiteralPath $database
}

# connect
Open-SQLite $database
$global:DbConnection = $db.Connection
$global:DbProviderFactory = $db.Factory

# create tables and add some data
Set-SQLite <#sql#>@'
PRAGMA foreign_keys = ON;

CREATE TABLE [TestCategories]
(
	[CategoryId] INTEGER PRIMARY KEY,
	[Category] TEXT NOT NULL,
	[Remarks] TEXT NULL
);

CREATE TABLE [TestNotes]
(
	[NoteId] INTEGER PRIMARY KEY,
	[CategoryId] INTEGER NOT NULL,
	[Note] TEXT NOT NULL,
	[Created] [datetime] NULL,
	FOREIGN KEY(CategoryId) REFERENCES TestCategories(CategoryId) ON DELETE RESTRICT
);

INSERT INTO TestCategories (Category, Remarks) VALUES ('Task', 'Task remarks');
INSERT INTO TestCategories (Category, Remarks) VALUES ('Warning', 'Warning remarks');
INSERT INTO TestCategories (Category, Remarks) VALUES ('Information', 'Information remarks');

INSERT INTO TestNotes (CategoryId, Note, Created) VALUES (1, 'Try to modify, insert and delete records.', datetime('now'));
INSERT INTO TestNotes (CategoryId, Note, Created) VALUES (2, 'Do <CtrR> before using of just inserted records.', datetime('now'));
INSERT INTO TestNotes (CategoryId, Note, Created) VALUES (3, '<Enter> on Category field opens TestCategories.', datetime('now'));
INSERT INTO TestNotes (CategoryId, Note, Created) VALUES (3, 'Run Initialize-Test.far.ps1 again for initial test data.', datetime('now'));
'@
