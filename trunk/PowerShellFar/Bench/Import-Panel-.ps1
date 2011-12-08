
<#
.Synopsis
	Imports objects from files and shows them in a panel.
	Author: Roman Kuzmin

.Description
	A single object is opened in a Member panel, two and more objects are
	opened in an Object panel.

.Example
	Far Commands | File associations:
	Mask: *.clixml;*.csv
	Command: >: Import-Panel- (Get-FarPath) #
#>

param
(
	# Same as the -Path parameter of Import-{Clixml|Csv} cmdlets.
	$Path = $(throw "Missed parameter -Path.")
	,
	# Panel columns.
	$Columns
	,
	# Clixml|Csv|Txt, or files should have extensions .clixml|.csv|.txt
	$Format
)
if ($args) { throw "Unknown parameters: $args" }

if (!$Format) {
	switch -regex ($Path) {
		'\.clixml$' { $Format = 'Clixml'; break }
		'\.csv$' { $Format = 'Csv'; break }
		'\.txt$' { $Format = 'Txt'; break }
		default { throw "Unknown file extension. Use the -Format parameter." }
	}
}

switch($Format) {
	'Clixml' { $obj = @(Import-Clixml -Path $Path); break }
	'Csv' { $obj = @(Import-Csv -Path $Path); break }
	'Txt' { $obj = @(Import-Csv -Path $Path -Delimiter "`t"); break }
	default { throw "Parameter -Format: unknown value: $Format." }
}

if ($obj.Count -eq 0) {
	$Far.Message('No objects')
}
elseif ($obj.Count -eq 1) {
	# show object members (MemberPanel)
	Open-FarPanel $obj[0] -Title $Path
}
else {
	# show several objects (ObjectPanel)
	$obj | Out-FarPanel -Columns $Columns -Title $Path
}
