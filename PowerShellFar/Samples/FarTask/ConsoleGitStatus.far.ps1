<#
.Synopsis
	Prints brief git status when current directory changes.

.Description
	This script watches for changes of the current directory and prints brief
	git status. Note, most of processing is in the task thread with only two
	tiny Far jobs.

	The tool is designed to run at most one instance. On the first run it
	starts watching and printing. On the second run it tells to stop the
	running task and exits.
#>

# if already running, set status to 'stop', print message, exit
if ([FarNet.User]::Data.ConsoleGitStatus -eq 'run') {
	[FarNet.User]::Data.ConsoleGitStatus = 'stop'
	Write-Host 'Stopping ConsoleGitStatus'
	return
}

# set status to 'run' and start the task
[FarNet.User]::Data.ConsoleGitStatus = 'run'
Start-FarTask {
	$LastPath = $null
	$LastInfo = $null

	# loop while not told to stop, with pauses after each step
	for(; [FarNet.User]::Data.ConsoleGitStatus -eq 'run'; Start-Sleep 2) {
		# job 1: take data from Far
		$path = job {
			if ($Far.Window.Kind -eq 'Panels') {
				$Far.CurrentDirectory
			}
		}

		# process data in the task thread
		if (!$path -or $LastPath -eq $path) {
			continue
		}
		$LastPath = $path
		$info = &{
			$ErrorActionPreference = 'Ignore'
			git.exe -C $path status -sb 2>$null
		}
		if (!$info) {
			continue
		}
		$branch, $status = $info
		$status = (
			$status | Group-Object {$_.Substring(0, 2)} -NoElement | Sort-Object Name |
			.{process{"$($_.Count)$($_.Name.Replace(' ', '-'))"}}
		) -join ' '
		$info = "$branch $status"

		# job 2: print to Far console
		if ($LastInfo -ne $info) {
			$LastInfo = $info
			ps: {
				$Var.info
			}
		}
	}
}
