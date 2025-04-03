<#
.Synopsis
	How to pass parameters in script blocks.

.Description
	(1) Define parameters in script blocks as usual.
	(2) Specify them as dynamic parameters for Start-FarTask.
	(!) Avoid switch parameters or specify after parameter Script.
#>

Start-FarTask -Param1 Hi -Param2 Joe {
	param($Param1, $Param2)

	# script parameters are defined
	if ($Param1 -ne 'Hi' -or $Param2 -ne 'Joe') {throw}

	# key values are added to $Data
	if ($Data.Param1 -ne 'Hi' -or $Data.Param2 -ne 'Joe') {throw}

	job {
		# key values are added to $Data
		$Far.Message($Data.Param1 + ' ' + $Data.Param2)
	}
}
