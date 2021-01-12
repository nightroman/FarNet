<#
.Synopsis
	How to pass parameters in task script files.

.Description
	(1) Define parameters in the script file as usual.
	(2) Specify them as dynamic parameters for Start-FarTask:
		Start-FarTask Parameters.fas.ps1 -Param1 hi -Param2 there
	(!) Switch parameters must be specified after the parameter Script.
#>

param(
	$Param1 = 'hello',
	$Param2 = 'world'
)

job -Arguments $Param1, $Param2 {
	$Far.Message($args[0] + ' ' + $args[1])
}
