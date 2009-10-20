
<#
.SYNOPSIS
	Creates, exports, imports and sends data to a panel.
	Author: Roman Kuzmin

.DESCRIPTION
	Creates Test-Zoo.clixml in the current location. It is not removed because
	this file is useful as a sample of CLIXML for other tests and experiments.
#>

# compile dynamically
$cs = (Split-Path $MyInvocation.MyCommand.Definition) + '\Test-Zoo.cs'
Add-Type ([IO.File]::ReadAllText($cs))

# create an object defined in .NET (fixed types)
$dno = New-Object Test.Zoo

# create a custom object by PowerShell (not fixed types)
$pso = New-Object PSObject -Property @{
	name = "User data"
	bool_ = $false
	double_ = 0.0
	int_ = 0
	long_ = 0L
	strings = @('Power', 'Shell')
}

# export created data to clixml
$dno, $pso | Export-Clixml Test-Zoo.clixml

# import data from clixml, change names
$zoo = Import-Clixml Test-Zoo.clixml
foreach($e in $zoo) { $e.name += ' (Imported)' }

# one more object with null properties
$pso2 = New-Object PSObject -Property @{
	Name = 'With null data'
	Data1 = $null
	Data2 = $null
	Data3 = $null
}

# put original and restored data to Object panel
$dno, $pso, $zoo[0], $zoo[1], $pso2 | New-FarObjectPanel | Start-FarPanel -OrderBy 'Name'
