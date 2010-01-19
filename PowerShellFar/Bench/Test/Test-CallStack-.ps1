
<#
.SYNOPSIS
	Test prompt dialog and shows how to get call stack info.
	Author: Roman Kuzmin

.DESCRIPTION
	Stack information can be viewed on errors if $ErrorActionPreference is set
	to Inquire. In this mode an error opens a PowerShell choice dialog. Click
	[Suspend] and in the editor console type one of the commands:

	Get-PSCallStack
	Get-PSCallStack | Format-List

	Alternatively, an editor console can be opened from the plugin menu [F11].
#>

# enable inquire dialog on errors
$ErrorActionPreference = 'Inquire'

# this function fails if $prm is 0
function FailWithZero($prm)
{
	if ($prm -eq 0) {
		# Error: Cannot remove variable Far ...
		Remove-Variable Far
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

# use try block just for test sake
try {
	Test 2
}
catch {
}
