
<#
.Synopsis
	Initializes test environment and optional database data.
	Author: Roman Kuzmin

.Description
	Supported database providers for testing:
	'System.Data.SQLite', 'System.Data.SqlClient', 'System.Data.SqlServerCe'
#>

param
(
	[string]$DbProviderName
	,
	[switch]$NoDb
)

# check the host
if ($Host.Name -ne 'FarHost') { throw "Invoke this script by FarHost." }

# add this directory to the system path for this session
$path = ';' + (Split-Path $MyInvocation.MyCommand.Path)
if ($env:PATH -notlike "*$path*") {
	$env:PATH += $path
}

# no DB?
if ($NoDb) { return }

### select a provider if not yet
if (!$DbProviderName) {
	'System.Data.SQLite', 'System.Data.SqlClient', 'System.Data.SqlServerCe' | Out-FarList -Title "Database Provider"
	if (!$DbProviderName) {
		return
	}
}

# SQLite setup
function SetupSQLite
{
	$null = [System.Reflection.Assembly]::LoadWithPartialName('System.Data.SQLite')
	$global:DbProviderFactory = [System.Data.SQLite.SQLiteFactory]::Instance

	$DbPath = Join-Path $env:TEMP Tempdb.sqlite
	$ConnectionString = "Data Source=`"$DbPath`"; FailIfMissing=False"
	if (Test-Path $DbPath) { Remove-Item $DbPath }

	# open the database connection
	$global:DbConnection = $DbProviderFactory.CreateConnection()
	$DbConnection.ConnectionString = $ConnectionString
	$DbConnection.Open()
}

# SqlClient setup
function SetupSqlClient
{
	$null = [System.Reflection.Assembly]::LoadWithPartialName('System.Data.SqlClient')
	$global:DbProviderFactory = [System.Data.SqlClient.SqlClientFactory]::Instance

	# create and open database connection (Tempdb)
	$global:DbConnection = $DbProviderFactory.CreateConnection()
	$DbConnection.ConnectionString = "Data Source=.\sqlexpress;Initial Catalog=Tempdb;Integrated Security=SSPI;"
	$DbConnection.Open()

	# execute command that creates two test tables in Tempdb and adds some data
	$c = $DbConnection.CreateCommand()
	$c.CommandText = @'
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TestNotes]'))
DROP TABLE [TestNotes]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TestCategories]'))
DROP TABLE [TestCategories]
'@
	$null = $c.ExecuteNonQuery()
	$c.Dispose()
}

# SqlServerCe setup
function SetupSqlServerCe
{
	$null = [System.Reflection.Assembly]::LoadWithPartialName('System.Data.SqlServerCe')
	$DbProviderFactory = [System.Data.SqlServerCe.SqlCeProviderFactory]::Instance

	$DbPath = Join-Path $env:TEMP Tempdb.sdf
	$ConnectionString = "Data Source=`"$DbPath`""
	if (Test-Path $DbPath) { Remove-Item $DbPath }

	# create factory, engine and database
	$engine = New-Object System.Data.SqlServerCe.SqlCeEngine($ConnectionString)
	$engine.CreateDatabase()
	$engine.Dispose()

	# open the database connection
	$global:DbConnection = $DbProviderFactory.CreateConnection()
	$DbConnection.ConnectionString = $ConnectionString
	$DbConnection.Open()
}

### setup and open connection; this step depends on a provider
switch($DbProviderName) {
	'System.Data.SQLite' { SetupSQLite; break }
	'System.Data.SqlClient' { SetupSqlClient; break }
	'System.Data.SqlServerCe' { SetupSqlServerCe; break }
	default { throw "Unsupported provider: $DbProviderName" }
}

### create tables and add some data; this step is common
$commands = @( # command set

if ($DbProviderName -eq 'System.Data.SQLite') { ### SQLite commands

'PRAGMA foreign_keys = ON'

# create table TestCategories
@'
CREATE TABLE [TestCategories]
(
[CategoryId] INTEGER PRIMARY KEY,
[Category] [nvarchar](100) NOT NULL,
[Remarks] [ntext] NULL
)
'@

# add some records
"INSERT INTO TestCategories (CategoryId, Category, Remarks) VALUES (NULL, 'Task', 'Task remarks')"
"INSERT INTO TestCategories (CategoryId, Category, Remarks) VALUES (NULL, 'Warning', 'Warning remarks')"
"INSERT INTO TestCategories (CategoryId, Category, Remarks) VALUES (NULL, 'Information', 'Information remarks')"

# create table TestNotes
@'
CREATE TABLE [TestNotes]
(
[NoteId] INTEGER PRIMARY KEY,
[CategoryId] INTEGER NOT NULL,
[Note] [ntext] NOT NULL,
[Created] [datetime] NULL,
FOREIGN KEY(CategoryId) REFERENCES TestCategories(CategoryId) ON DELETE RESTRICT
)
'@

# add some records
"INSERT INTO TestNotes (NoteId, CategoryId, Note, Created) VALUES (NULL, 1, 'Try to modify, insert and delete records.', datetime('now'))"
"INSERT INTO TestNotes (NoteId, CategoryId, Note, Created) VALUES (NULL, 2, 'Do <CtrR> before using of just inserted records.', datetime('now'))"
"INSERT INTO TestNotes (NoteId, CategoryId, Note, Created) VALUES (NULL, 3, '<Enter> on Category field opens TestCategories.', datetime('now'))"
"INSERT INTO TestNotes (NoteId, CategoryId, Note, Created) VALUES (NULL, 3, 'Run Initialize-Test-.ps1 again for initial test data.', datetime('now'))"

} else { ### SQL commands

# create table TestCategories
@'
CREATE TABLE [TestCategories]
(
[CategoryId] [int] IDENTITY(1,1) NOT NULL,
[Category] [nvarchar](100) NOT NULL,
[Remarks] [ntext] NULL,
CONSTRAINT [PK_TestCategories] PRIMARY KEY ([CategoryId])
)
'@

# add some records
"INSERT TestCategories (Category, Remarks) VALUES ('Task', 'Task remarks')"
"INSERT TestCategories (Category, Remarks) VALUES ('Warning', 'Warning remarks')"
"INSERT TestCategories (Category, Remarks) VALUES ('Information', 'Information remarks')"

# create table TestNotes
@'
CREATE TABLE [TestNotes]
(
[NoteId] [int] IDENTITY(1,1) NOT NULL,
[CategoryId] [int] NOT NULL,
[Note] [ntext] NOT NULL,
[Created] [datetime] NULL,
CONSTRAINT [PK_TestNotes] PRIMARY KEY ([NoteId])
)
'@

# add constraint
'ALTER TABLE [TestNotes] ADD CONSTRAINT [CategoryOfNote] FOREIGN KEY([CategoryId]) REFERENCES [TestCategories] ([CategoryId])'

# add some records
"INSERT TestNotes (CategoryId, Note, Created) VALUES (1, 'Try to modify, insert and delete records.', GETDATE())"
"INSERT TestNotes (CategoryId, Note, Created) VALUES (2, 'Do <CtrR> before using of just inserted records.', GETDATE())"
"INSERT TestNotes (CategoryId, Note, Created) VALUES (3, '<Enter> on Category field opens TestCategories.', GETDATE())"
"INSERT TestNotes (CategoryId, Note, Created) VALUES (3, 'Run Initialize-Test-.ps1 again for initial test data.', GETDATE())"
}
)

### execute commands (*)
foreach($CommandText in $commands) {
	$c = $DbConnection.CreateCommand()
	$c.CommandText = $CommandText
	$null = $c.ExecuteNonQuery()
	$c.Dispose()
}

<#
(*) With try/catch/finally ExecuteNonQuery used to fails in unit step mode. Why?
#>
