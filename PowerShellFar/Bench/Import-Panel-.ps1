
<#
.Synopsis
	Imports objects from files and shows them in a panel.
	Author: Roman Kuzmin

.Parameter Path
		The Path parameter of Import-{Clixml|Csv} cmdlets.
.Parameter Columns
		The Columns parameter of the Out-FarPanel cmdlet.
.Parameter Format
		One of the format values: clixml, csv, txt. If it is omitted then a
		file should have one of the known extensions: .clixml, .csv, .txt.

.Example
	Far Commands | File associations:
	Mask: *.clixml;*.csv
	Command: ps: Import-Panel-.ps1 (Get-FarPath) #
#>

param
(
	[Parameter(Mandatory = $true)]$Path,
	$Columns,
	$Format
)

if (!$Format) {
	switch -regex ($Path) {
		'\.clixml$' { $Format = 'Clixml' }
		'\.csv$' { $Format = 'Csv' }
		'\.txt$' { $Format = 'Txt' }
		default {throw "Unknown file extension. Use the -Format parameter."}
	}
}

switch($Format) {
	'clixml' { $obj = @(Import-Clixml -Path $Path) }
	'csv' { $obj = @(Import-Csv -Path $Path) }
	'txt' { $obj = @(Import-Csv -Path $Path -Delimiter "`t") }
	default {throw "Parameter -Format: unknown value: $Format."}
}

$obj | Out-FarPanel -Columns $Columns -Title $Path
