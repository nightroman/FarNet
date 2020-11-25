# How to pass parameters in a task script file.
# (1) Define parameters in the script file as usual.
# (2) Specify dynamic parameters for Start-FarTask:
#     Start-FarTask Parameters.fas.ps1 -Param1 hi -Param2 there
# (!) Switch parameters must be specified after the parameter Script.

param(
	$Param1 = 'hello',
	$Param2 = 'world'
)

$Data.text1 = $Param1
$Data.text2 = $Param2

job {
	$Far.Message($Data.text1 + ' ' + $Data.text2)
}
