<#
.Synopsis
	How to pass parameters in task script files.

.Description
	(1) Define parameters in the script file as usual.
	(2) Specify them as dynamic parameters for Start-FarTask:
		Start-FarTask Parameters.fas.ps1 -Param1 hi -Param2 there
	(!) Avoid switch parameters or specify them after parameter Script.
#>

param(
	$Param1 = 'hello',
	$Param2 = 'world'
)

# Jobs may access task variables as $Var.<name>
job {
	$Far.Message($Var.Param1 + ' ' + $Var.Param2)
}
