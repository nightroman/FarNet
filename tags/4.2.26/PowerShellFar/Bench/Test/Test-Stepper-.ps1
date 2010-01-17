
<#
.SYNOPSIS
	Test super-macro engine.
	Author: Roman Kuzmin

.DESCRIPTION
	This script starts processing of step units. A step unit is a script that
	returns steps: a sequence of keys and script blocks posted and invoked one
	by one. Far Manager gets control after each step, completes pending jobs
	and invokes the next step. This scenario allows to perform quite tricky
	operations impossible during normal continuous code flow.

	This approach is also useful for automated testing and this script is an
	example of a simple test monitor: it adds test units, starts processing,
	watches stepper events and writes information into a log file.

	For demo sake by default it shows a confirmation dialog before each step,
	so that you can see steps in progress. Use -Auto to disable.

.NOTES
	The script uses Trace-Far for some demo tracing. Trace listeners normally
	should be added in "Far.exe.config".
#>

param
(
	[switch]
	# Tells to process without user confirmations, as usual.
	$Auto
)

# Remove existing errors before the test
$Error.Clear()

# Create a stepper
$stepper = New-Object PowerShellFar.Stepper
$stepper.Ask = !$Auto

# Post units
$myFolder = Split-Path $MyInvocation.MyCommand.Path
$stepper.PostUnit("$myFolder\Test-Stepper+.ps1")
$stepper.PostUnit("$myFolder\Test-Dialog+.ps1")

# Add a handler to watch stepping progress
$stepper.add_StateChanged({

	# trace any change
	Trace-Far "Unit: '$($this.CurrentUnit)' $($this.State)..."

	# trace error on Failed
	if ($this.State -eq 'Failed') {
		Trace-Far $this.Error -Error
	}

	# case Completed or Failed:
	if ($this.State -eq 'Completed' -or $this.State -eq 'Failed') {

		# trace summary
		Trace-Far @"
$(Get-Date) Stepper stopped.
Processed steps: $($this.StepCount)
"@

		# end
		Remove-Item Variable:\Stepper*
	}
})

### Trace using Trace-Far
Trace-Far "$(Get-Date) Stepper started."

# Go! Normally this should be the last script command.
$stepper.Go()
