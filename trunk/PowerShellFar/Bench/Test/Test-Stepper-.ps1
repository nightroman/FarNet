
<#
.Synopsis
	Test the stepper engine.
	Author: Roman Kuzmin

.Description
	This script starts processing of step units. A step unit is a script that
	returns steps: a sequence of macros and script blocks posted and invoked
	one by one. The core gets control after each step, completes pending jobs
	and invokes the next step. This way is used to perform tricky operations
	impossible with synchronous code flow.

	This approach is useful for automated testing. This script is the example
	of a simple test monitor: it adds test units, starts processing, watches
	and traces stepper events.

	In order to invoke a single step unit without events just use the cmdlet
	Invoke-FarStepper, this is simpler than direct use of the class Stepper.

	For demo sake by default it shows a confirmation dialog before each step,
	so that you can see steps in progress.

.Parameter Auto
		Tells to process without confirmations.
#>

param
(
	[switch]$Auto
)

# Remove existing errors before the test
$Error.Clear()

# Create a stepper
$stepper = New-Object PowerShellFar.Stepper
$stepper.Ask = !$Auto

# Add units
$myFolder = Split-Path $MyInvocation.MyCommand.Path
$stepper.AddFile("$myFolder\Test-Stepper!.ps1")
$stepper.AddFile("$myFolder\Test-Dialog!.ps1")

# Add a handler to watch stepping progress
$stepper.add_StateChanged({

	# trace any state change
	[Diagnostics.Trace]::TraceInformation("Unit: '$($this.CurrentUnit)' $($this.State)...")

	# trace error on Failed
	if ($this.State -eq 'Failed') {
		[Diagnostics.Trace]::TraceError($this.Error)
	}

	# case Completed or Failed:
	if ($this.State -eq 'Completed' -or $this.State -eq 'Failed') {

		# trace summary
		[Diagnostics.Trace]::TraceInformation(@"
$(Get-Date) Stepper stopped.
Processed steps: $($this.StepCount)
"@)
	}
})

# Trace
[Diagnostics.Trace]::TraceInformation("$(Get-Date) Stepper started.")

# Go! Normally this should be the last script command.
$stepper.Go()
