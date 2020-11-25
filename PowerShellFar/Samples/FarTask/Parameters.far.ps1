# How to pass parameters in a task script block or code.
# (1) Define parameters in script block/code as usual.
# (2) Specify dynamic parameters for Start-FarTask:
#     Start-FarTask {...} -Param1 hi -Param2 there
# (!) Switch parameters must be specified after the parameter Script.

$text1 = 'hello'
$text2 = 'world'

Start-FarTask -Param1 $text1 -Param2 $text2 {
	param(
		$Param1,
		$Param2
	)

	$Data.text1 = $Param1
	$Data.text2 = $Param2

	job {
		$Far.Message($Data.text1 + ' ' + $Data.text2)
	}
}
