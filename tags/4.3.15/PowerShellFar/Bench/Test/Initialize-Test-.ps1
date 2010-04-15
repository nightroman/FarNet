
<#
.SYNOPSIS
	Initializes test environment and optional database data.
	Author: Roman Kuzmin
#>

param
(
	[string]$DbProviderName,
	[switch]$NoDb
)

# check Far
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
	# find supported data provider classes and ask to select one
	$DbProviderName = [Data.Common.DbProviderFactories]::GetFactoryClasses() | .{process{
		if ($_.InvariantName -eq 'System.Data.SqlClient' -or $_.InvariantName -eq 'System.Data.SqlServerCe.3.5') {
			$_.InvariantName
		}
	}} |
	Out-FarList -Title "Select database provider"
	if (!$DbProviderName) {
		return
	}
}

### get defined provider factory by name and set it global
$global:DbProviderFactory = [Data.Common.DbProviderFactories]::GetFactory($DbProviderName)

# SqlClient setup
function SetupSqlClient
{
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
	'System.Data.SqlClient' { SetupSqlClient; break }
	'System.Data.SqlServerCe.3.5' { SetupSqlServerCe; break }
}

### create tables and add some data; this step is common
$commands = @( # command set

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
