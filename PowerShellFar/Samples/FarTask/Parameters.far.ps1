<#
.Synopsis
	How to pass variables in task script blocks.

.Description
	Use the parameter Variable in order to import existing variables from the
	current session to the task session. Note, unlike file scripts, script
	blocks cannot be invoked with dynamic parameters.
#>

# variables in the current session
$text1 = 'hello'
$text2 = 'world'

# start task with imported variables
Start-FarTask -Variable text1, text2 {

	# use imported variables, keep results in $Data
	$Data.text = "$text1 $text2"

	# call some job using $Data
	job {
		$Far.Message($Data.text)
	}
}
