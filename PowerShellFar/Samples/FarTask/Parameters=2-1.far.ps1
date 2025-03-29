<#
.Synopsis
	Script parameters specified as dynamic, v9.0.10.
#>

Start-FarTask -Param1 Hi -Param2 Joe {
	param($Param1, $Param2)

	# script parameters are defined
	if ($Param1 -ne 'Hi' -or $Param2 -ne 'Joe') {throw}

	# key values are added to $Data
	if ($Data.Param1 -ne 'Hi' -or $Data.Param2 -ne 'Joe') {throw}

	job {
		# key values are added to $Data
		$Far.Message($Data.Param1 + ', ' + $Data.Param2)
	}
}
