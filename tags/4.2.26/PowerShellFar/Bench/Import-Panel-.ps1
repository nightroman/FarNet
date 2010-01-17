
<#
.SYNOPSIS
	Imports objects from files to a panel
	Author: Roman Kuzmin

.DESCRIPTION
	A single object is opened in Member panel, two and more objects are opened
	in Object panel.

.EXAMPLE
	Far Commands|File associations:
	Mask: *.clixml
	Command: >: Import-Panel- (Get-FarPath) #
#>

param
(
	# Same as -Path of Import-{Clixml|Csv}.
	$Path = $(throw "Missed parameter -Path."),

	# Property name or scriptblock to be used for Name column.
	$FarName,

	# 'Clixml' or 'Csv', or files should have extensions .clixml or .csv
	$Format
)
if ($args) { throw "Unknown parameters: $args" }

if (!$Format) {
	switch -regex ($Path) {
		'\.clixml$' { $Format = 'Clixml'; break }
		'\.csv$' { $Format = 'Csv'; break }
		default { throw "Unknown file extension and missed parameter -Format." }
	}
}

switch($Format) {
	'Clixml' { $obj = @(Import-Clixml -Path $Path); break }
	'Csv' { $obj = @(Import-Csv -Path $Path); break }
	default { throw "Parameter -Format: unknown value: $Format." }
}

if ($obj.Count -eq 0) {
	$Far.Msg('No objects')
}
elseif ($obj.Count -eq 1) {
	# show object members (MemberPanel)
	Start-FarPanel $obj[0] -Title $Path
}
else {
	# show several objects (ObjectPanel)
	$obj | New-FarObjectPanel -FarName $FarName | Start-FarPanel -Title $Path
}
