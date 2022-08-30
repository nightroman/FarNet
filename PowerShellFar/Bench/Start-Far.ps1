<#
.Synopsis
	Starts Far with the command, panels, options.
	Author: Roman Kuzmin

.Description
	The script starts Far in a new console with shown panels, optionally with
	specified paths, and invokes the specified command in the active panel.

.Parameter Command
		Specifies the command to be invoked in the active panel.
		Note, commands starting with ":" are ignored.

.Parameter Panel1
		Specifies the active panel path.
		By default it is the current location.

.Parameter Panel2
		Specifies the passive panel path.
		By default it is the active panel path.

.Parameter Title
		Specifies the window title.

.Parameter ReadOnly
		Tells to start with read only profile data.

.Parameter Wait
		Tells to wait for exit and set $LASTEXITCODE.

.Parameter Quit
		Tells to quit Far when the command completes.

.Parameter Timeout
		Specifies the timeout interval in milliseconds.
		The exit code is set to this number on timeout.

.Parameter Environment
		Specifies the environment variables dictionary.

.Example
	> ps: Start-Far 'ps:$Psf.StartCommandConsole()' $Far.Panel.CurrentDirectory $Far.Panel2.CurrentDirectory
#>

[CmdletBinding()]
param(
	[string]$Command,
	[string]$Panel1 = '.',
	[string]$Panel2 = $Panel1,
	[string]$Title,
	[switch]$ReadOnly,
	[switch]$Wait,
	[switch]$Quit,
	[int]$Timeout,
	[hashtable]$Environment
)

trap {$PSCmdlet.ThrowTerminatingError($_)}
$ErrorActionPreference = 1

if ($Command) {
	### Normal call for starting Far with the command

	$Panel1 = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Panel1)
	$Panel2 = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Panel2)
	if (!(Test-Path -LiteralPath $Panel1)) {throw "Missing path 1: '$Panel1'."}
	if (!(Test-Path -LiteralPath $Panel2)) {throw "Missing path 2: '$Panel2'."}

	$Environment = if ($Environment) {[hashtable]::new($Environment)} else {@{}}
	$Environment.FAR_START_COMMAND = $Command
	$Environment.FAR_START_PANEL1 = $Panel1
	$Environment.FAR_START_PANEL2 = $Panel2
	$Environment.FAR_START_QUIT = if ($Quit) {1} else {$null}
	$Environment.FAR_START_TIMEOUT = if ($Timeout -gt 0) {$Timeout} else {$null}
	$oldEnvironment = @{}
	foreach($_ in $Environment.GetEnumerator()) {
		$oldEnvironment.Add($_.Key, [System.Environment]::GetEnvironmentVariable($_.Key))
		[System.Environment]::SetEnvironmentVariable($_.Key, $_.Value)
	}

	try {
		$exe = if ($env:FARHOME) {"$env:FARHOME\Far.exe"} else {'Far.exe'}
		$arg = $(
			'"ps: Start-FarTask \"{0}\""' -f $MyInvocation.MyCommand.Path
			if ($Title) {"/title:`"$Title`""}
			if ($ReadOnly) {'/ro'}
		)
		$p = Start-Process $exe $arg -PassThru
		if ($Wait) {
			$p.WaitForExit()
			$global:LASTEXITCODE = $p.ExitCode
		}
	}
	finally {
		foreach($_ in $oldEnvironment.GetEnumerator()) {
			[System.Environment]::SetEnvironmentVariable($_.Key, $_.Value)
		}
	}
}
else {
	### Internall call in started Far by Start-FarTask

	if (!$env:FAR_START_COMMAND) {throw 'Please specify the command.'}

	# setup panels
	job {
		$Far.Panel.IsVisible = $true
		$Far.Panel2.IsVisible = $true
		if ($env:FAR_START_PANEL1) {
			if ([System.IO.Directory]::Exists($env:FAR_START_PANEL1)) {
				$Far.Panel.CurrentDirectory = $env:FAR_START_PANEL1
			}
			else {
				$Far.Panel.GoToPath($env:FAR_START_PANEL1)
			}
		}
		if ($env:FAR_START_PANEL2) {
			if ([System.IO.Directory]::Exists($env:FAR_START_PANEL2)) {
				$Far.Panel2.CurrentDirectory = $env:FAR_START_PANEL2
			}
			else {
				$Far.Panel2.GoToPath($env:FAR_START_PANEL2)
			}
		}
	}

	# start timer
	if ($env:FAR_START_TIMEOUT) {
		$timer = [System.Timers.Timer]::new($env:FAR_START_TIMEOUT)
		$null = Register-ObjectEvent -InputObject $timer -EventName Elapsed -Action {[System.Environment]::Exit($env:FAR_START_TIMEOUT)}
		$timer.Start()
	}

	# invoke command
	if (!$env:FAR_START_COMMAND.StartsWith(':')) {
		job {
			$Far.CommandLine.Text = $env:FAR_START_COMMAND
		}
		keys Enter
	}

	# quit
	if ($env:FAR_START_QUIT -eq 1) {
		job {
			$Far.Quit()
		}
	}
}
