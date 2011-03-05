
<#
.SYNOPSIS
	Panel tables for a connection.
	Author: Roman Kuzmin

.DESCRIPTION
	Requires a $DbProviderFactory instance and an open connection object as
	$DbConnection.

	Theoretically it should work for any ADO.NET data provider. But it was
	tested only for SQL Server, SQL Server Compact, MS Access, PostgreSQL.
	Still, it may not work for some providers with not standard SQL syntax.
	Please, report such cases.

	PANEL ACTIONS

	[Enter]
	Opens a child table panel with its records (Panel-DbData-.ps1).

	[CtrlPgDn]
	Opens a child member panel with table properties.
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
	# Tells to close the connection.
	$CloseConnection
)

if (!$DbProviderFactory) { throw "Provider factory is not defined." }
if (!$DbConnection) { throw "Connection is not defined." }

# get table of tables
if ($DbProviderFactory -is [System.Data.OleDb.OleDbFactory]) {
	$table = $DbConnection.GetSchema('Tables')
}
else {
	$adapter = $DbProviderFactory.CreateDataAdapter()
	$command = $DbConnection.CreateCommand()
	$command.CommandText = "SELECT * FROM INFORMATION_SCHEMA.TABLES"
	$adapter.SelectCommand = $command
	$table = New-Object System.Data.DataTable 'Tables'
	$null = $adapter.Fill($table)
	$adapter.Dispose()
}

# get column names
$cname = $DbConnection.GetType().Name
if ($cname -eq 'SqlConnection' -or $cname -eq 'SqlCeConnection' -or $cname -eq 'OleDbConnection' -or $cname -eq 'NpgsqlConnection') {
	# Providers with 3 common columns
	$columns = 'TABLE_NAME', 'TABLE_SCHEMA', @{ Expression = 'TABLE_TYPE'; Width = 12 }
}
else {
	# ODBC, Oracle and hopefully others
	$columns = $table | Get-Member -MemberType Property '*NAME*', '*SCHEM*', '*TYPE*', '*OWNER*' | .{process{ $_.Name }}
}

### create and configure a user panel for table objects
$Panel = New-Object PowerShellFar.ObjectPanel
$Panel.Title = "$($DbConnection.Database) Tables"
$Panel.Columns = $columns
$Panel.Data['66e6fa15-150f-450e-baa1-e7e0bf19c6e1'] = @{ DbProviderFactory = $DbProviderFactory; DbConnection = $DbConnection }

# garbage
$Panel.Garbage.Add($table)
if ($CloseConnection) { $Panel.Garbage.Add($DbConnection) }

### set [Enter] handler ([CtrlPgDn] is for members)
$Panel.AsOpenFile = {
	$pd = $this.Data['66e6fa15-150f-450e-baa1-e7e0bf19c6e1']
	$fd = $_.File.Data
	$table = $fd.TABLE_NAME
	# Npgsql: quote table names to include tables with case sensitive names
	if ($pd.DbConnection.GetType().Name -eq 'NpgsqlConnection') {
		$select = "SELECT * FROM $($fd.TABLE_SCHEMA).`"$table`""
	}
	# SqlServer:
	elseif ($fd.TABLE_SCHEMA -isnot [System.DBNull]) {
		$select = "SELECT * FROM $($fd.TABLE_SCHEMA).$($table)"
	}
	# SqlServerCe:
	else {
		$select = "SELECT * FROM [$table]"
	}
	# open child table data panel
	Panel-DbData- -AsChild -SelectCommand $select -Title $table -DbProviderFactory $pd.DbProviderFactory -DbConnection $pd.DbConnection
}

# go!
$Panel.AddObjects(($table.Rows | Sort-Object 'TABLE_NAME'))
$Panel.Open($AsChild)
