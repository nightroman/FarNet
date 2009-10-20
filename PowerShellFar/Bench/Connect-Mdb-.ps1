
<#
.SYNOPSIS
	Connects .mdb database and optionally panels its tables.
	Author: Roman Kuzmin
#>

param
(
	[Parameter(Mandatory=$true)]
	[string]
	# .mdb file path.
	$Path,

	[string]
	# Connection options.
	$Options,

	[switch]
	# Panel database tables.
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
