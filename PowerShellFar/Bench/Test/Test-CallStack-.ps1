
<#
.SYNOPSIS
	Test call stack information.
	Author: Roman Kuzmin

.DESCRIPTION
	Stack information can be viewed on errors if $ErrorActionPreference is set
	to Inquire. In this mode an error opens a PowerShell choice dialog. Such a
	dialog (not only error) in PowerShellFar has an extra feature: [Esc] shows
	call stack and error records in the internal viewer.
#>

# enable inquire dialog on errors
$ErrorActionPreference = 'Inquire'

# this function fails if $prm is 0
function FailWithZero($prm)
{
	if ($prm -eq 0) {
		Remove-Variable 'fake name'
	}
}

# passing 0 in Invert triggers an error
function Test($prm)
{
	if ($prm -ge 0) {
		FailWithZero $prm
		--$prm
		Test $prm
	}
}

# use try block for test sake
try {
	Test 2
}
catch {
}
