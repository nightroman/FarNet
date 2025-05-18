<#
.Synopsis
	How FarTask may handle quitting.

.Description
	How to use:
	- Register [F10] macro, see README.
	- Run this and see steps counting from 1 to 10.
	- Hit [F10] in the middle or after and see what happens.

	This script is deliberately verbose in order to show techniques.
	Real scripts may be simpler.

	$Info is used by the handler (main thread) and the task (background).
	Real scripts may have to take care of conflicts.
#>

# Tracing.
function global:print {$Far.UI.WriteLine($args)}

# Shared state.
$Info = @{
	Id = ++$Global:TestQuitWithFarTaskLastId
	Step = 0
}

# Register quitting handler, use GetNewClosure() to capture local variables ($Info).
# Use captured $Info and $_ [FarNet.QuittingEventArgs].
$Info.Registration = [FarNet.User]::RegisterQuitting({
	# Is quitting already cancelled by others?
	if ($_.Ignore) {return}

	# UI
	$answer = Show-FarMessage -Caption Quitting -Choices Quit, Remove, Continue @"
Task-$($Info.Id) at step $($Info.Step)
Task is running. Choice?
"@

	# Quit
	if ($answer -eq 0) {
		return
	}

	# Remove
	if ($answer -eq 1) {
		print "Task-$($Info.Id) cancel quitting, remove handler"
		$_.Ignore = $true
		$Info.Registration.Dispose()
		return
	}

	# Continue or [Esc]
	print "Task-$($Info.Id) just cancel quitting"
	$_.Ignore = $true
}.GetNewClosure())

# Start task, pass $Info by variable name, use as `$Data.Info`.
print "Task-$($Info.Id) start steps"
Start-FarTask -Data Info {
	for($$ = 0; $$ -lt 10; ++$$) {
		job {
			print "Task-$($Data.Info.Id) do step $($Data.Info.Step)"
		}

		if ($Data.Info.Registration.IsDisposed) {
			job {
				print "Task-$($Data.Info.Id) stop steps"
			}
			return
		}

		++$Data.Info.Step
		Start-Sleep 1
	}

	job {
		print "Task-$($Data.Info.Id) remove handler, all steps done"
		$Data.Info.Registration.Dispose()
	}
}
