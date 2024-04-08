<#
.Synopsis
	How to pass variables or hastables in task script blocks.

.Description
	Use -Data to expose variables and dictionaries as the special variable $Data.
#>

# variables in the current session
$variable1 = 'Hello'
$hashtable1 = @{user = 'Joe'; id = 42}

# start task with the variable and hashtable exposed as $Data
Start-FarTask -Data variable1, $hashtable1 {
	job {
		$Far.Message($Data.variable1 + ' ' + $Data.user + ' (' + $Data.id + ')')
	}
}
