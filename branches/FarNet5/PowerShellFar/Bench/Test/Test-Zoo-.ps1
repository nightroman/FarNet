
<#
.Synopsis
	Creates, exports, imports and sends data to a panel.
	Author: Roman Kuzmin

.Description
	Creates Test-Zoo.clixml in $HOME. It is not removed after this test because
	the file is useful as a sample of CLIXML for other tests and experiments.
#>

function ScriptRoot { Split-Path $MyInvocation.ScriptName }

# compile dynamically definition of a class
Add-Type ([IO.File]::ReadAllText("$(ScriptRoot)\Test-Zoo.cs"))

# create an object defined in .NET (strong typed members)
$dno = New-Object Test.Zoo

# create a custom object by PowerShell (weak typed members)
$pso = New-Object PSObject -Property @{
	name = "User data"
	bool_ = $false
	double_ = 0.0
	int_ = 0
	long_ = 0L
	strings = @('Power', 'Shell')
}

# export created data to clixml
$dno, $pso | Export-Clixml "$HOME\Test-Zoo.clixml"

# import data from clixml, change names
$zoo = Import-Clixml "$HOME\Test-Zoo.clixml"
foreach($e in $zoo) { $e.name += ' (Imported)' }

# one more object with null properties
$pso2 = New-Object PSObject -Property @{
	Name = 'With null data'
	Data1 = $null
	Data2 = $null
	Data3 = $null
}

# send original and deserialized data to a panel
$dno, $pso, $zoo[0], $zoo[1], $pso2 | Out-FarPanel -SortMode 'Name'
