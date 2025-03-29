<#
.Synopsis
	Script parameters specified as variables, v9.0.10.
#>

$Param1 = 'Hi'
$Param2 = 'Joe'

Start-FarTask {
	param($Param1, $Param2)

	# script parameters are not defined
	if ($Param1 -ne $null -or $Param2 -ne $null) {throw}

	# key values are added to $Data
	if ($Data.Param1 -ne 'Hi' -or $Data.Param2 -ne 'Joe') {throw}

	job {
		# key values are added to $Data
		$Far.Message($Data.Param1 + ', ' + $Data.Param2)
	}
}
