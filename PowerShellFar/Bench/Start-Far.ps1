<#
.Synopsis
	Starts Far with the command, panels, options.
	Author: Roman Kuzmin

.Description
	The script starts Far in a new console with shown panels, optionally with
	specified paths, and invokes the specified command in the active panel.

	Parameters Test and Timeout are useful for running FarNet commands as tools
	or tests. Note that they automatically set ReadOnly and Wait to true.

.Parameter Command
		Specifies the command to be invoked in the active panel.
		Commands starting with ":" are ignored (void or disabled).

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
		Tells to wait for exit, check for the exit code, and fail if it is not 0.

.Parameter Quit
		Tells to quit Far when the command completes.

.Parameter Test
		Tells to invoke the command as a tool or test and exit after the
		specified time in milliseconds. The exit code is 1 if the command
		throws any exception and 0 otherwise.

		Use 0 for immediate exit or some positive value in order to see the
		command results or errors for some time.

		This parameter only works for FarNet commands.
		Commands must be synchronous and non interactive.

		ReadOnly and Wait are set to true.

.Parameter Timeout
		Tells to exit if the command runs longer than the specifies time in
		milliseconds. The exit code is set to the timeout value.

		ReadOnly and Wait are set to true.

.Parameter Environment
		Specifies the environment variables dictionary.

.Parameter WindowStyle
		Specifies the value for Start-Process -WindowStyle.

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
	[int]$Test = -1,
	[int]$Timeout = -1,
	[hashtable]$Environment,
	[System.Diagnostics.ProcessWindowStyle]$WindowStyle = 'Normal'
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
	$Environment.FAR_START_TEST = if ($Test -ge 0) {$ReadOnly = $Wait = $true; $Test} else {$null}
	$Environment.FAR_START_TIMEOUT = if ($Timeout -ge 1) {$ReadOnly = $Wait = $true; $Timeout} else {$null}
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
		$p = Start-Process $exe $arg -WindowStyle $WindowStyle -PassThru
		if ($Wait) {
			$p.WaitForExit()
			$global:LASTEXITCODE = $p.ExitCode
			if ($global:LASTEXITCODE) {
				throw "Command exited with code $global:LASTEXITCODE -- $Command"
			}
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

	if (!$env:FAR_START_COMMAND) {
		throw 'Please specify the command or use ":" for void.'
	}

	if ($env:FAR_START_TEST) {
		[FarNet.Works.Test]::SetTestCommand($env:FAR_START_TEST)
	}

	if ($env:FAR_START_TIMEOUT) {
		[FarNet.Works.Test]::SetTimeout($env:FAR_START_TIMEOUT)
	}

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

	# invoke command unless void or disabled
	if (!$env:FAR_START_COMMAND.StartsWith(':')) {
		job {
			$Far.CommandLine.Text = $env:FAR_START_COMMAND
		}
		keys Enter
	}

	# quit
	if ($env:FAR_START_QUIT) {
		job {
			$Far.Quit()
		}
	}
}
