
<#
.Synopsis
	Test debugger and breakpoints.
	Author: Roman Kuzmin

.Description
	This test shows examples of a few breakpoint types: command, variable
	(reading, writing, reading and writing) and custom action breakpoints.

	- Run it and some breakpoints in this script will be set for testing.
	- Run it again to see how the debugger works when breakpoints are hit.
	- After testing you may want to remove these breakpoints:
	>: .\Test-Debugger.ps1 -RemoveBreakpoints

	Tested on: FarHost, ConsoleHost, PowerShell ISE V2 CTP3.
#>

[CmdletBinding()]
param
(
	# To remove breakpoints
	[switch]$RemoveBreakpoints
)

$script = $MyInvocation.MyCommand.Definition

# get my breakpoints
$bp = Get-PSBreakpoint | Where-Object { $_.Script -eq $script }

### Case: remove breakpoints
if ($RemoveBreakpoints) {
	$bp | Remove-PSBreakpoint
	Write-Host "Removed $($bp.Count) breakpoints."
	return
}

### Case: set breakpoints
if (!$bp)
{
	# command breakpoint, e.g. function TestStepIntoOverOut
	$null = Set-PSBreakpoint -Script $script -Command 'TestStepIntoOverOut'

	# variable breakpoint on reading
	$null = Set-PSBreakpoint -Script $script -Variable 'varRead' -Mode Read

	# variable breakpoint on writing
	$null = Set-PSBreakpoint -Script $script -Variable 'varWrite' -Mode Write

	# variable breakpoint on reading and writing
	$null = Set-PSBreakpoint -Script $script -Variable 'varReadWrite' -Mode ReadWrite

	# special breakpoint with action without breaking (for logging, diagnostics and etc.)
	# NOTE: mind infinite recursion (stack overflow) if the action accesses the same variables
	$null = Set-PSBreakpoint -Script $script -Variable 'varRead', 'varWrite', 'varReadWrite' -Mode ReadWrite -Action {
		++$script:VarAccessCount
	}

	Write-Host @'

Test breakpoints have been set.
Invoke this script again to hit them.

'@
	return
}

### Case: proceed with breakpoints

# will be counted by the breakpoint action
$script:VarAccessCount = 0

# function to test steps: into, over, out
function TestStepIntoOverOut
{
	$_ = 3 # See me in debugger? Then you have stepped into.
	$_ = 7 # See me in debugger? Then you have not stepped out or continued.
}

# to test step into, over, out
TestStepIntoOverOut # break on command; try: step into, over

# to change variable in debugger
[int]$toWrite = 0 # change me after
if ($toWrite -le 0) {
	Write-Host "Nothing to write."
}
else {
	Write-Host "Writing"
	1..$toWrite
}

# to see steps
if ($true -and $true -and $true) {} # step through the expression

$varRead = 1 # no break
$_ = $varRead # break on reading

$varWrite = 2 # break on writing
$_ = $varWrite # no break

$varReadWrite = 3 # break on writing
$_ = $varReadWrite # break on reading

# counter value is calculated by the breakpoint action
Write-Host @"

Test is done.
Watched variables have been accessed $($script:VarAccessCount) times.
"@
