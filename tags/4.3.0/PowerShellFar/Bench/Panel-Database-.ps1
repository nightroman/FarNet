
<#
.SYNOPSIS
	Panel SQL server databases.
	Author: Roman Kuzmin

.DESCRIPTION
	Uses existing SQL server connection or opens a new connection assuming SQL
	Server Express ".\sqlexpress" instance.
#>

if (!$DbProviderFactory -or !$DbConnection) {
	$DbProviderFactory = [Data.SqlClient.SqlClientFactory]::Instance
	$DbConnection = $DbProviderFactory.CreateConnection()
	$DbConnection.ConnectionString = "Data Source=.\sqlexpress;Initial Catalog=Master;Integrated Security=SSPI;"
	$DbConnection.Open()
}

$p = Panel-DbData- -NoShow -SelectCommand 'sp_helpdb'
$p.SetOpen({
	$DbProviderFactory = [Data.SqlClient.SqlClientFactory]::Instance
	$DbConnection = $DbProviderFactory.CreateConnection()
	$DbConnection.ConnectionString = "Data Source=.\sqlexpress;Initial Catalog=$($_.File.Name);Integrated Security=SSPI;"
	$DbConnection.Open()
	Panel-DbTable- -AsChild -Title $_.File.Name
})

if ($local:DbConnection) { $p.Garbage.Add($DbConnection) }
Start-FarPanel $p -Title "Databases"
