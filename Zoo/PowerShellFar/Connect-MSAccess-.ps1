
<#
.Synopsis
	Connects .mdb database and optionally shows tables in a panel.
	Author: Roman Kuzmin

.Description
	With -Panel switch the script shows database tables in a panel using
	Panel-DbTable-.ps1 and closes the connection together with a panel.

	Otherwise the script has to be dot-sourced. It opens the connection and
	creates result variables $DbConnection and $DbProviderFactory in the
	current scope. $DbConnection is used and closed by a caller.
#>

param
(
	[Parameter(Mandatory=$true)]
	[string]
	# .mdb file path.
	$Path
	,
	[string]
	# Connection options.
	$Options
	,
	[switch]
	# To show tables in a panel.
	$Panel
)

# create and open connection
$DbProviderFactory = [System.Data.OleDb.OleDbFactory]::Instance
$DbConnection = $DbProviderFactory.CreateConnection()
$DbConnection.ConnectionString = @"
Provider=Microsoft.Jet.OLEDB.4.0;Data Source="$Path";$Options
"@
$DbConnection.Open()

# show panel with tables
if ($Panel) {
	Panel-DbTable- -CloseConnection
}
