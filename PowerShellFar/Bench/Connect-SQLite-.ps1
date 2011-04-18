
<#
.SYNOPSIS
	Connects SQLite database and optionally shows tables in a panel.
	Author: Roman Kuzmin

.DESCRIPTION
	Requires System.Data.SQLite ADO.NET provider: http://system.data.sqlite.org
	Install it or just put System.Data.SQLite.dll to the FarNet home directory.
	Starting with 1.0.69 SQLite.Interop.dll is also needed.

	With -Panel switch the script shows database tables in a panel using
	Panel-DbTable-.ps1 and closes the connection together with a panel.

	Otherwise the script has to be dot-sourced. It opens the connection and
	creates result variables $DbConnection and $DbProviderFactory in the
	current scope. $DbConnection is used and closed by a caller.

	If -Panel is specified and -Options is empty then it prompts for options.
	This might be needed for example in order to set "DateTimeFormat=Ticks".

	Far Manager file accosiation to open a database in the panel:
	SQLite database file
	Mask: *.sqlite;*.db3;*.db
	Command: >: Connect-SQLite- (Get-FarPath) -Panel #
#>

param
(
	[Parameter(Mandatory=$true)]
	[string]
	# SQLite DB file path.
	$Path
	,
	[string]
	# Connection options.
	$Options
	,
	[string]
	# Registered provider name (e.g. 'System.Data.SQLite').
	$ProviderName
	,
	[switch]
	# To show tables in a panel.
	$Panel
)

### get factory
if ($ProviderName) {
	$DbProviderFactory = [System.Data.Common.DbProviderFactories]::GetFactory($ProviderName)
}
else {
	$null = [System.Reflection.Assembly]::LoadWithPartialName('System.Data.SQLite')
	$DbProviderFactory = [System.Data.SQLite.SQLiteFactory]::Instance
}

### ask for options
if ($Panel -and !$Options) {
	$Options = $Far.Input("Options", "Connection.SQLite", "SQLite connection")
}

### open connection
$DbConnection = $DbProviderFactory.CreateConnection()
$DbConnection.ConnectionString = @"
Data Source = "$Path"; $Options
"@
$DbConnection.Open()

# enforce foreign keys
& {
	$command = $DbConnection.CreateCommand()
	$command.CommandText = 'PRAGMA foreign_keys=ON'
	$null = $command.ExecuteNonQuery()
	$command.Dispose()
}

### show tables in a panel
if ($Panel) {
	Panel-DbTable- -CloseConnection
}
