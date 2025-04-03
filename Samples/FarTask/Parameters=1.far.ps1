<#
.Synopsis
	How to pass data in task scripts.

.Description
	Use parameter -Data to add variables and hashtables to $Data.
#>

# some variables
$variable1 = 'Hi'
$hashtable1 = @{user = 'Joe'; id = 42}

# start task with the variable and hashtable added to $Data
Start-FarTask -Data variable1, $hashtable1 {
	# use $Data in task
	if ($Data.variable1 -ne 'Hi' -or $Data.user -ne 'Joe') { throw }

	job {
		# use $Data in jobs
		$Far.Message($Data.variable1 + ' ' + $Data.user + ' (' + $Data.id + ')')
	}
}
