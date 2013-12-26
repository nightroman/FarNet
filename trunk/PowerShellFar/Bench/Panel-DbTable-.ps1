
<#
.Synopsis
	Panel database tables for the specified connection.
	Author: Roman Kuzmin

.Description
	THIS IS USEFUL BUT YOU CAN KILL YOUR DATA, TOO. USE IT ON YOUR OWN RISK.

	For most providers, and normally when a table has a primary key, you can
	insert, delete, modify, and update records in the database.

	The script was tried with the following providers:
	-- SQL Server Express
	-- SQL Server Compact
	-- SQLite
	-- MS Access
	-- PostgreSQL

	It may not work with providers with specific SQL syntax or features.
	Please, report such cases and known solutions.

	The script requires the DbProviderFactory and the open connection
	DbConnection. They are either predefined variables or parameters.

	PANEL ACTIONS

	[Enter]
	Shows the input box with the default SELECT statement for the table. Press
	[Esc] to cancel. The SELECT statement can be modified or even replaced with
	a different SQL command. ANY COMMAND IS EXECUTED AS IT IS, e.g. if you type
	DROP TABLE X then you get it done. [Enter] executes the SQL statement from
	the input box and opens a panel with the result records using the script
	Panel-DbData-.ps1.

	[CtrlPgDn]
	Opens a panel with the current table properties.

.Parameter DbProviderFactory
		Data provider factory.
		Default: predefined variable $DbProviderFactory.
.Parameter DbConnection
		Database connection.
		Default: predefined variable $DbConnection.
.Parameter AsChild
		Tells to open the panel as a child of the current panel.
.Parameter CloseConnection
		Tells to close the connection on the panel exit.
#>

param
(
	[System.Data.Common.DbProviderFactory]$DbProviderFactory = $DbProviderFactory,
	[System.Data.Common.DbConnection]$DbConnection = $DbConnection,
	[switch]$AsChild,
	[switch]$CloseConnection
)
$ErrorActionPreference = 'Stop'
if (!$DbProviderFactory) { Write-Error 'Provider factory is not defined.' }
if (!$DbConnection) { Write-Error 'Connection is not defined.' }

function Table($Sql) {
	$command = $DbConnection.CreateCommand()
	$command.CommandText = $Sql
	$adapter = $DbProviderFactory.CreateDataAdapter()
	$adapter.SelectCommand = $command
	$table = [System.Data.DataTable]'Tables'
	$null = $adapter.Fill($table)
	$adapter.Dispose()
	, $table
}

### Get table of tables and views
$provider = $DbConnection.GetType().Name
if ($provider -eq 'OdbcConnection' -or $provider -eq 'OleDbConnection') {
	$table = $DbConnection.GetSchema('Tables')
}
elseif ($provider -eq 'SQLiteConnection') {
	$table = Table "SELECT name AS TABLE_NAME, type AS TABLE_TYPE, sql AS SQL FROM sqlite_master WHERE type = 'table' OR type = 'view'"
}
else {
	$table = Table "SELECT * FROM INFORMATION_SCHEMA.TABLES"
}

### Get panel columns
if ($provider -eq 'SqlConnection' -or $provider -eq 'SqlCeConnection' -or $provider -eq 'OleDbConnection' -or $provider -eq 'NpgsqlConnection') {
	# Providers with 3 common columns
	$columns = 'TABLE_NAME', 'TABLE_SCHEMA', @{ Expression = 'TABLE_TYPE'; Width = 12 }
}
elseif ($provider -eq 'SQLiteConnection') {
	# SQLite
	$columns = 'TABLE_NAME', 'TABLE_TYPE', 'SQL'
}
else {
	# ODBC, Oracle, and hopefully others
	$columns = $table | Get-Member -MemberType Property '*NAME*', '*SCHEM*', '*TYPE*', '*OWNER*' | .{process{ $_.Name }}
}

### Open object panel with table/view rows
$Panel = New-Object PowerShellFar.ObjectPanel
$Panel.Explorer.Functions = 'GetContent, OpenFile'
$Panel.Title = "$($DbConnection.Database) Tables"
$Panel.Columns = $columns
$Panel.Data['66e6fa15-150f-450e-baa1-e7e0bf19c6e1'] = @{ DbProviderFactory = $DbProviderFactory; DbConnection = $DbConnection }

# garbage
if ($CloseConnection) { $Panel.Garbage.Add($DbConnection) }

# go
$Panel.AddObjects(($table.Rows | Sort-Object 'TABLE_NAME'))
$Panel.Open($AsChild)

### [Enter] handler ([CtrlPgDn] is for members)
$Panel.AsOpenFile = {
	param($0, $_)
	$data = $0.Data['66e6fa15-150f-450e-baa1-e7e0bf19c6e1']
	$DbProviderFactory = $data.DbProviderFactory
	$DbConnection = $data.DbConnection
	$row = $_.File.Data
	$table = $row.TABLE_NAME
	$provider = $DbConnection.GetType().Name

	# Npgsql: quote table names to include tables with case sensitive names
	if ($provider -eq 'NpgsqlConnection') {
		$select = "SELECT * FROM $($row.TABLE_SCHEMA).`"$table`""
	}
	# SqlServer:
	elseif ($provider -eq 'SqlConnection' -and $row.TABLE_SCHEMA -isnot [System.DBNull]) {
		$select = "SELECT * FROM $($row.TABLE_SCHEMA).$($table)"
	}
	# SQLite:
	# SqlServerCe:
	else {
		$select = "SELECT * FROM [$table]"
	}

	# prompt for the command
	$select = $Far.Input('Command text', 'Connection.Select', 'SQL SELECT', $select)
	if ($select) {
		# open child table data panel
		Panel-DbData- -AsChild -SelectCommand:$select -Title:$table -DbProviderFactory:$DbProviderFactory -DbConnection:$DbConnection
	}
}
