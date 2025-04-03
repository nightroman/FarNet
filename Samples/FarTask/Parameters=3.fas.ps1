<#
.Synopsis
	How to pass parameters in script files.

.Description
	(1) Define parameters in the script file as usual.
	(2) Specify them as dynamic parameters for Start-FarTask.
	(!) Avoid switch parameters or specify after parameter Script.

.Example
	Start-FarTask .\Parameters=3.fas.ps1 -Param1 Hi -Param2 Joe
#>

param(
	$Param1 = 'Hello',
	$Param2 = 'World'
)

# Jobs may access task variables as $Var.<name>
job {
	$Far.Message($Var.Param1 + ' ' + $Var.Param2)
}
