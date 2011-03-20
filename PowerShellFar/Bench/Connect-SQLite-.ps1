
<#
.SYNOPSIS
	Connects SQLite database and optionally shows tables in a panel.
	Author: Roman Kuzmin

.DESCRIPTION
	With -Panel switch the script shows database tables in a panel using
	Panel-DbTable-.ps1 and closes the connection together with a panel.

	Otherwise the script has to be dot-sourced. It opens the connection and
	creates result variables $DbConnection and $DbProviderFactory in the
	current scope. $DbConnection is used and closed by a caller.

	File accosiation to open the panel:
	SQLite database file
	Mask: *.sqlite;*.db3;*.db
	Command: >: Connect-SQLite- (Get-FarPath) -Panel #

.LINK
	http://sourceforge.net/projects/sqlite-dotnet2
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
	# Provider name (may depend on version).
	$ProviderName = 'System.Data.SQLite'
	,
	[switch]
	# To show tables in a panel.
	$Panel
)

# create and open connection
$DbProviderFactory = [System.Data.Common.DbProviderFactories]::GetFactory($ProviderName)
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

# show panel with tables
if ($Panel) {
	Panel-DbTable- -CloseConnection
}
