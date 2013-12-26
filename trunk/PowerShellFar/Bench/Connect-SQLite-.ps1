
<#
.Synopsis
	Connects SQLite database and optionally shows tables in a panel.
	Author: Roman Kuzmin

.Description
	SQLite prerequisites:
	1. Get the NuGet package System.Data.SQLite
	2. Copy its three net20 or net40 DLLs as
	-- <path>\System.Data.SQLite.dll
	-- <path>\x64\SQLite.Interop.dll
	-- <path>\x86\SQLite.Interop.dll
	3. Set the env variable SystemDataSQLite to <path>\System.Data.SQLite.dll

	With the switch Panel the script shows database tables in a panel using
	Panel-DbTable-.ps1 and closes the connection when the panel is closed.

	Otherwise the script has to be dot-sourced. It opens the connection and
	creates result variables $DbConnection and $DbProviderFactory in the
	current scope. $DbConnection is used and closed by a caller.

	If Panel is specified and Options is empty then it prompts for options.
	This might be needed for example in order to set "DateTimeFormat=Ticks".

	Note: "Foreign Keys" is set to true by default.

	Far Manager file accosiation to open a database in the panel:
	SQLite database file
	Mask: *.sqlite;*.db3;*.db
	Command: ps: Connect-SQLite-.ps1 (Get-FarPath) -Panel #

.Parameter Path
		SQLite database file path.
.Parameter Options
		Options, use the connection string format.
		Note: "Foreign Keys" is set to true by default.
.Parameter Panel
		Tells to open the panel to browse the database tables.
#>

param
(
	[Parameter(Mandatory=$true)]
	[string]$Path,
	[string]$Options,
	[switch]$Panel
)

Assert-Far $env:SystemDataSQLite 'Please, set the env variable SystemDataSQLite.'

# get factory
$env:PreLoadSQLite_UseAssemblyDirectory = 1
$null = [System.Reflection.Assembly]::LoadFile($env:SystemDataSQLite)
$DbProviderFactory = [System.Data.SQLite.SQLiteFactory]::Instance

# get options
if ($Panel -and !$Options) {
	$Options = $Far.Input('Options', 'Connection.SQLite', 'SQLite connection')
}

# open connection
$DbConnection = $DbProviderFactory.CreateConnection()
$DbConnection.ConnectionString = &{
	$builder = $DbProviderFactory.CreateConnectionStringBuilder()
	$builder.set_ConnectionString($Options)
	$builder['Data Source'] = $Path
	if (!$builder.ContainsKey('Foreign Keys')) { $builder['Foreign Keys'] = $true }
	$builder.ConnectionString
}
$DbConnection.Open()

# open panel
if ($Panel) {
	Panel-DbTable- -CloseConnection
}
