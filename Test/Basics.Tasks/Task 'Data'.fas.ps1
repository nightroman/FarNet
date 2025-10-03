<#
.Synopsis
	Test task/job $Data.

.Notes
	Keep " " and "'" in file name for test sake.
#>

# $Data is the automatic variable
Assert-Far @(
	$Data -is [Hashtable]
	$Data.Count -eq 0
)

job {
	Assert-Far @(
		$Data -is [Hashtable]
		$Data.Count -eq 0
	)
}
job {
	# set a new value
	$Data.Test1 = 42
}
job {
	# check the value, case insensitive
	Assert-Far @(
		$Data.Test1 -eq 42
		$Data.ContainsKey('TEST1')
	)

	# yet another value
	$Data.Test2 = 55

	# kill the data
	$Data = 12345
	Assert-Far $Data -eq 12345
}
job {
	# it is alive and has the new value
	Assert-Far $Data.Test2 -eq 55
}
