
<#
.SYNOPSIS
	Panel tables for a connection.
	Author: Roman Kuzmin

.DESCRIPTION
	THIS IS USEFUL BUT YOU CAN KILL YOUR DATA, TOO. USE IT ON YOUR OWN RISK.

	For most providers, and normally when a table has a primary key, you can
	insert, delete, modify and update records in the source database.

	The script was tried with the following providers:
	-- SQL Server Express
	-- SQL Server Compact
	-- SQLite
	-- MS Access
	-- PostgreSQL

	It may not work with providers with not standard SQL syntax or features.
	Please, report such cases and known solutions.

	The script requires a $DbProviderFactory instance and an open connection
	$DbConnection. They are either predefined variables or the parameters.

	PANEL ACTIONS

	[Enter]
	Shows an input box with the default SELECT statement for the table. Press
	[Esc] to cancel. The SELECT statement can be modified or even replaced with
	a different SQL command. Any command is executed as it is, if you type DROP
	TABLE X then you get it done. All in all, [Enter] executes the final SQL
	statement from the input box and opens a panel with the result records.
	This is done by the Panel-DbData-.ps1 script.

	[CtrlPgDn]
	Opens a panel with the current table properties.
#>

param
(
	[System.Data.Common.DbProviderFactory]
	# Data provider factory instance. Default: variable $DbProviderFactory is expected.
	$DbProviderFactory = $DbProviderFactory
	,
	[System.Data.Common.DbConnection]
	# Database connection. Default: variable $DbConnection is expected.
	$DbConnection = $DbConnection
	,
	[switch]
	# Show the panel as a child panel of the current panel.
	$AsChild
	,
	[switch]
	# Tells to close the connection on the panel exit.
	$CloseConnection
)

if (!$DbProviderFactory) { throw "Provider factory is not defined." }
if (!$DbConnection) { throw "Connection is not defined." }

function Table($Sql) {
	$command = $DbConnection.CreateCommand()
	$command.CommandText = $Sql
	$adapter = $DbProviderFactory.CreateDataAdapter()
	$adapter.SelectCommand = $command
	$table = New-Object System.Data.DataTable 'Tables'
	$null = $adapter.Fill($table)
	$adapter.Dispose()
	, $table
}

### Get the table of table and view records
$cname = $DbConnection.GetType().Name
if ($cname -eq 'OdbcConnection' -or $cname -eq 'OleDbConnection') {
	$table = $DbConnection.GetSchema('Tables')
}
elseif ($cname -eq 'SQLiteConnection') {
	$table = Table "SELECT name AS TABLE_NAME, type AS TABLE_TYPE, sql AS SQL FROM sqlite_master WHERE type = 'table' OR type = 'view'"
}
else {
	$table = Table "SELECT * FROM INFORMATION_SCHEMA.TABLES"
}

### Get panels columns
if ($cname -eq 'SqlConnection' -or $cname -eq 'SqlCeConnection' -or $cname -eq 'OleDbConnection' -or $cname -eq 'NpgsqlConnection') {
	# Providers with 3 common columns
	$columns = 'TABLE_NAME', 'TABLE_SCHEMA', @{ Expression = 'TABLE_TYPE'; Width = 12 }
}
elseif ($cname -eq 'SQLiteConnection') {
	# SQLite
	$columns = 'TABLE_NAME', 'TABLE_TYPE', 'SQL'
}
else {
	# ODBC, Oracle, and hopefully others
	$columns = $table | Get-Member -MemberType Property '*NAME*', '*SCHEM*', '*TYPE*', '*OWNER*' | .{process{ $_.Name }}
}

### Create and open an object panel with table/view rows
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
	$data = $this.Data['66e6fa15-150f-450e-baa1-e7e0bf19c6e1']
	$DbProviderFactory = $data.DbProviderFactory
	$DbConnection = $data.DbConnection
	$row = $_.File.Data
	$table = $row.TABLE_NAME
	$cname = $DbConnection.GetType().Name

	# Npgsql: quote table names to include tables with case sensitive names
	if ($cname -eq 'NpgsqlConnection') {
		$select = "SELECT * FROM $($row.TABLE_SCHEMA).`"$table`""
	}
	# SqlServer:
	elseif ($cname -eq 'SqlConnection' -and $row.TABLE_SCHEMA -isnot [System.DBNull]) {
		$select = "SELECT * FROM $($row.TABLE_SCHEMA).$($table)"
	}
	# SQLite:
	# SqlServerCe:
	else {
		$select = "SELECT * FROM [$table]"
	}

	# prompt for the select command
	$select = $Far.Input("Command text", "Connection.Select", "SQL SELECT", $select)
	if ($select) {
		# open child table data panel
		Panel-DbData- -AsChild -SelectCommand $select -Title $table -DbProviderFactory $DbProviderFactory -DbConnection $DbConnection
	}
}
