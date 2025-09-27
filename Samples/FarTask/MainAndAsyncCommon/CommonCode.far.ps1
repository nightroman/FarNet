<#
.Synopsis
	This code may be called from main and async sessions.
	See "Test-CommonCode.far.ps1"
#>

# shared data
$Data = @{
	var1 = 1
}

# test, increment, print
ps: {
	Write-Host "Testing common code." -ForegroundColor Cyan
	Assert-Far $Data.var1 -eq 1
	++$Data.var1
	$Data.var1
}

# test, increment, return
$result = job {
	Assert-Far $Data.var1 -eq 2
	++$Data.var1
	$Data.var1
}

# test result
if ($result -ne 3) {throw "Expected: 3, actual: $result"}

# test and print
ps: {
	Assert-Far $Data.var1 -eq 3
	$Data.var1
}
