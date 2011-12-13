
<#
.Synopsis
	Connects SQLite database and optionally shows tables in a panel.
	Author: Roman Kuzmin

.Description
	Requires System.Data.SQLite ADO.NET provider: http://system.data.sqlite.org
	Install it to GAC or copy SQLite assemblies to the FarNet directory.

	With -Panel switch the script shows database tables in a panel using
	Panel-DbTable-.ps1 and closes the connection together with a panel.

	Otherwise the script has to be dot-sourced. It opens the connection and
	creates result variables $DbConnection and $DbProviderFactory in the
	current scope. $DbConnection is used and closed by a caller.

	If -Panel is specified and -Options is empty then it prompts for options.
	This might be needed for example in order to set "DateTimeFormat=Ticks".

	Note: database foreign keys are enabled by the script on connection.

	Far Manager file accosiation to open a database in the panel:
	SQLite database file
	Mask: *.sqlite;*.db3;*.db
	Command: >: Connect-SQLite- (Get-FarPath) -Panel #
#>

param
(
	[Parameter(Mandatory=$true)]
	[string]
	# SQLite database file path.
	$Path
	,
	[string]
	# Options, connection string format.
	$Options
	,
	[string]
	# Registered provider name (e.g. 'System.Data.SQLite').
	$ProviderName
	,
	[switch]
	# Tells to open the panel to browse the database tables.
	$Panel
)

### the factory
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
$DbConnection.ConnectionString = &{
	$builder = $DbProviderFactory.CreateConnectionStringBuilder()
	$builder.set_ConnectionString($Options)
	$builder['data source'] = $Path
	$builder['foreign keys'] = $true
	$builder.ConnectionString
}
$DbConnection.Open()

### open the panel with tables
if ($Panel) {
	Panel-DbTable- -CloseConnection
}
