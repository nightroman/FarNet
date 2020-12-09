<#
.Synopsis
	How to pass variables in task script blocks.

.Description
	Use the parameter Data in order to import existing variables from the
	current session to the task $Data. Note, unlike file scripts, script
	blocks cannot be invoked with dynamic parameters.
#>

# variables in the current session
$text1 = 'hello'
$text2 = 'world'

# start task with variables passed via $Data
Start-FarTask -Data text1, text2 {
	job {
		$Far.Message($Data.text1 + ' ' + $Data.text2)
	}
}
