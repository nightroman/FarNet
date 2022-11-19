<#
.Synopsis
	Opens SQLite database panel.
	Author: Roman Kuzmin

.Description
	Requires the package FarNet.SQLite.

	The script shows database tables in a panel using Panel-DBTable.ps1.

	If Options is empty then it prompts for options, e.g. "DateTimeFormat=Ticks".
	Note: "Foreign Keys" is set to true by default.

	Far Manager file accosiation to open a database:
	- Mask: *.sqlite;*.db3;*.db
	- Command: ps: Panel-SQLite.ps1 (Get-FarPath)

.Parameter Database
		Specifies the SQLite database file path.

.Parameter Options
		Specifies options in the connection string format.
		Note: "Foreign Keys" is set to true by default.
#>

[CmdletBinding()]
param(
	[Parameter(Mandatory=$true)]
	[string]$Database
	,
	[string]$Options
)

$ErrorActionPreference=1
try {
	Import-Module $env:FARHOME\FarNet\Lib\FarNet.SQLite

	# get options
	if (!$Options) {
		$Options = $Far.Input('Options', 'Connection.SQLite', 'SQLite connection')
	}

	# connect
	Open-SQLite $Database $Options -ForeignKeys

	# open panel
	Panel-DBTable -CloseConnection -DbConnection $db.Connection -DbProviderFactory $db.Factory
}
catch {
	Write-Error $_
}
