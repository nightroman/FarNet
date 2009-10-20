
<#
.SYNOPSIS
	Connects .sdf database and optionally panels its tables.
	Author: Roman Kuzmin
#>

param
(
	[Parameter(Mandatory=$true)]
	[string]
	# .sdf file path.
	$Path,

	[string]
	# Connection options (see MSDN SqlCeConnection.ConnectionString).
	$Options,

	[string]
	# Provider name (may depend on version)
	$ProviderName = 'System.Data.SqlServerCe.3.5',

	[switch]
	# Panel database tables.
	$Panel
)

# create and open connection
$DbProviderFactory = [System.Data.Common.DbProviderFactories]::GetFactory($ProviderName)
$DbConnection = $DbProviderFactory.CreateConnection()
$DbConnection.ConnectionString = @"
Data Source = "$Path"; $Options
"@
$DbConnection.Open()

# show panel with tables
if ($Panel) {
	Panel-DbTable- -CloseConnection
}
