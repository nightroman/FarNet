
<#
.Synopsis
	Panel SQL server databases.
	Author: Roman Kuzmin

.Description
	Uses existing SQL server connection or opens a new connection assuming SQL
	Server Express ".\sqlexpress" instance.

.Notes
	sp_helpdb may fail on the first call after reboot if there is a missing
	database. "SELECT * FROM sys.databases" works fine and shows problem
	databases, too. Besides, it gets more database info (but size).
#>

if (!$DbProviderFactory -or !$DbConnection) {
	$DbProviderFactory = [Data.SqlClient.SqlClientFactory]::Instance
	$DbConnection = $DbProviderFactory.CreateConnection()
	$DbConnection.ConnectionString = "Data Source=.\sqlexpress;Initial Catalog=Master;Integrated Security=SSPI;"
	$DbConnection.Open()
}

$Panel = Panel-DbData- -NoShow -SelectCommand 'SELECT * FROM sys.databases' -Columns 'name', 'database_id', 'state_desc', 'create_date'
$Panel.AsOpenFile = {
	param($0, $_)
	$DbProviderFactory = [Data.SqlClient.SqlClientFactory]::Instance
	$DbConnection = $DbProviderFactory.CreateConnection()
	$DbConnection.ConnectionString = "Data Source=.\sqlexpress;Initial Catalog=$($_.File.Name);Integrated Security=SSPI;"
	$DbConnection.Open()
	Panel-DbTable- -AsChild -Title $_.File.Name
}

if ($local:DbConnection) { $Panel.Garbage.Add($DbConnection) }
Open-FarPanel $Panel -Title "Databases"
