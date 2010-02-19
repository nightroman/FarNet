
<#
.SYNOPSIS
	Panel DataTable rows.
	Author: Roman Kuzmin

.DESCRIPTION
	Shows table records or result rows of a SELECT command and allows to
	UPDATE, INSERT, and DELETE real data right in a database (in some cases
	these operations should be configured manually through a data adapter).

	See [PowerShellFar.DataPanel] for details.
#>

param
(
	# Command to select data; it is a command text or a command object.
	$SelectCommand,

	[string]
	# Name of a database table. If empty -SelectCommand is used.
	$TableName,

	[System.Data.Common.DbProviderFactory]
	# Data provider factory instance. Default: variable $DbProviderFactory is expected.
	$DbProviderFactory = $DbProviderFactory,

	[System.Data.Common.DbConnection]
	# Database connection. Default: variable $DbConnection is expected.
	$DbConnection = $DbConnection,

	[System.Data.Common.DbDataAdapter]
	# Data adapter used for data manipulations (depends on connection).
	$DbDataAdapter,

	[switch]
	# To close the connection when a panel exits.
	$CloseConnection,

	[switch]
	# Show this panel as a child panel (can be omitted if -Lookup is used).
	$AsChild,

	[string]
	# Panel title.
	$Title,

	[string[]]
	# Columns to be shown
	$Columns,

	[string[]]
	# Fields to exclude in the child record panel.
	$ExcludeMembers,

	# Handler triggered on Enter in the lookup table.
	$Lookup,

	[switch]
	# Create and return a panel for later use.
	$NoShow
)

if ($args) { throw "Unknown parameters: $args" }
if (!$DbProviderFactory) { throw "Provider factory is not defined." }
if (!$DbConnection) { throw "Connection is not defined." }

# create a panel
$p = New-Object PowerShellFar.DataPanel
$p.Factory = $DbProviderFactory

# create adapter
if (!$DbDataAdapter) {
	$DbDataAdapter = $DbProviderFactory.CreateDataAdapter()
}

# setup select command
if ($TableName) {
	$DbDataAdapter.SelectCommand = $DbConnection.CreateCommand()
	$DbDataAdapter.SelectCommand.CommandText = "SELECT * FROM $TableName"
}
elseif ($SelectCommand -is [string]) {
	$DbDataAdapter.SelectCommand = $DbConnection.CreateCommand()
	$DbDataAdapter.SelectCommand.CommandText = $SelectCommand
}
elseif ($SelectCommand -is [System.Data.Common.DbCommand]) {
	$DbDataAdapter.SelectCommand = $SelectCommand
}
elseif ($DbDataAdapter.SelectCommand -eq $null) {
	throw "You have to set -TableName or -SelectCommand or SelectCommand in -Adapter"
}

# set adapter and objects to be disposed
$p.Adapter = $DbDataAdapter
if ($CloseConnection) { $p.Garbage.Add($DbConnection) }

# panel settings
$p.Columns = $Columns
$p.ExcludeMembers = $ExcludeMembers
$p.Panel.Info.Title = $Title
$p.SetLookup($Lookup)

# go!
if ($NoShow) {
	$p
}
else {
	$p.Show($AsChild -or $Lookup)
}