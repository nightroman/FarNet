<#
.Synopsis
	Panel DataTable rows.
	Author: Roman Kuzmin

.Description
	Shows table records or result rows of a SELECT command and provides UI to
	UPDATE, INSERT, and DELETE real data right in a database (in some cases
	these operations should be configured manually through a data adapter).

	See [PowerShellFar.DataPanel] for details.

.Parameter SelectCommand
		Command to select data; it is a command text or a command object.

.Parameter TableName
		Name of a database table. If empty -SelectCommand is used.

.Parameter DbProviderFactory
		Data provider factory instance. Default: variable $DbProviderFactory is expected.

.Parameter DbConnection
		Database connection. Default: variable $DbConnection is expected.

.Parameter DbDataAdapter
		Data adapter used for data manipulations (depends on connection).

.Parameter CloseConnection
		To close the connection when a panel exits.

.Parameter AsChild
		Show this panel as a child panel (can be omitted if -Lookup is used).

.Parameter Title
		Panel title.

.Parameter Columns
		Columns to be shown

.Parameter ExcludeMemberPattern
		Regex pattern to exclude fields in the child record panel.

.Parameter Lookup
		Handler triggered on Enter in the lookup table.

.Parameter NoShow
		Create and return a panel for later use.
#>

[CmdletBinding()]
param(
	[object]$SelectCommand
	,
	[string]$TableName
	,
	[System.Data.Common.DbProviderFactory]$DbProviderFactory = $DbProviderFactory
	,
	[System.Data.Common.DbConnection]$DbConnection = $DbConnection
	,
	[System.Data.Common.DbDataAdapter]$DbDataAdapter
	,
	[switch]$CloseConnection
	,
	[switch]$AsChild
	,
	[string]$Title
	,
	[string[]]$Columns
	,
	[string]$ExcludeMemberPattern
	,
	[object]$Lookup
	,
	[switch]$NoShow
)

if (!$DbProviderFactory) { throw "Provider factory is not defined." }
if (!$DbConnection) { throw "Connection is not defined." }

# create a panel
$Panel = [PowerShellFar.DataPanel]::new()

# create adapter
if (!$DbDataAdapter) {
	$DbDataAdapter = $DbProviderFactory.CreateDataAdapter()
	$Panel.Garbage.Add($DbDataAdapter)
}

# setup select command
if ($TableName) {
	$DbDataAdapter.SelectCommand = $DbConnection.CreateCommand()
	$DbDataAdapter.SelectCommand.CommandText = "SELECT * FROM $TableName"
	$Panel.Garbage.Add($DbDataAdapter.SelectCommand)
}
elseif ($SelectCommand -is [string]) {
	$DbDataAdapter.SelectCommand = $DbConnection.CreateCommand()
	$DbDataAdapter.SelectCommand.CommandText = $SelectCommand
	$Panel.Garbage.Add($DbDataAdapter.SelectCommand)
}
elseif ($SelectCommand -is [System.Data.Common.DbCommand]) {
	$DbDataAdapter.SelectCommand = $SelectCommand
}
elseif ($DbDataAdapter.SelectCommand -eq $null) {
	throw "You have to set -TableName or -SelectCommand or SelectCommand in -Adapter"
}

# panel data
$Panel.Factory = $DbProviderFactory
$Panel.Adapter = $DbDataAdapter
$Panel.Lookup = $Lookup

# panel view
$Panel.Title = $Title
$Panel.Columns = $Columns
$Panel.ExcludeMemberPattern = $ExcludeMemberPattern

# objects to be disposed
if ($CloseConnection) {
	$Panel.Garbage.Add($DbConnection)
}

# go!
if ($NoShow) {
	$Panel
}
else {
	if ($AsChild -or $Lookup) {
		$Panel.OpenChild($null)
	}
	else {
		$Panel.Open()
	}
}
