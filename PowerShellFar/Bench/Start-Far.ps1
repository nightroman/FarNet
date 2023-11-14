<#
.Synopsis
	Starts Far with the command, panels, options.
	Author: Roman Kuzmin

.Description
	The script starts Far in a new console with shown or hidden panels with
	optional paths and invokes the specified command in the active panel.

	Parameters Test and Timeout are useful for running FarNet commands as tools
	or tests. Note that they automatically set ReadOnly and Wait to true.

.Parameter Command
		Specifies the command to be invoked in the active panel.
		By default commands are treated as FarNet commands.
		Use the switch Enter for other commands.

.Parameter Panel1
		Specifies the active panel directory or file.
		Default: the current location.

.Parameter Panel2
		Specifies the passive panel directory or file.
		Default: the active panel directory or file.

.Parameter Title
		Specifies the window title.

.Parameter Enter
		Tells to enter the command as typed manually.
		The command may be native, plugin, or FarNet.

.Parameter Hidden
		Tells to start with hidden panels.
		Otherwise both panels are visible.

.Parameter ReadOnly
		Tells to start with read only profile data.

.Parameter Wait
		Tells to wait for exit, check for the exit code, and fail if it is not 0.

.Parameter Quit
		Tells to quit Far when the command completes.

.Parameter Test
		Tells to invoke the command as a test or tool and exit after the
		specified time in milliseconds. Exit code: 0 (success) or 1 (failure).

		Use 0 for immediate exit or some positive value in order to pause for
		seeing the command results or errors.

		This parameter only works for FarNet commands.
		Commands must be synchronous and non interactive.

		With Test, switches ReadOnly, Wait, Enter are set to true.

.Parameter Timeout
		Tells to exit if the command runs longer than the specifies time in
		milliseconds. The exit code is set to the timeout value.

		With Timeout, switches ReadOnly, Wait are set to true.

.Parameter Environment
		Specifies the environment variables dictionary.

.Parameter WindowStyle
		Specifies the value for Start-Process -WindowStyle.

.Example
	> ps: Start-Far 'ps:$Psf.StartCommandConsole()' -Hidden
#>

[CmdletBinding()]
param(
	[string]$Command,
	[string]$Panel1 = '.',
	[string]$Panel2 = $Panel1,
	[string]$Title,
	[switch]$Enter,
	[switch]$Hidden,
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
	$Environment.FARNET_PSF_START_SCRIPT = "Start-FarTask '$($MyInvocation.MyCommand.Path.Replace("'", "''"))'"
	$Environment.FARNET_PSF_START_PANEL1 = $Panel1
	$Environment.FARNET_PSF_START_PANEL2 = $Panel2
	$Environment.FAR_START_COMMAND = $Command
	$Environment.FAR_START_ENTER = if ($Enter -or $Test -ge 0) {1} else {$null}
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
			if ($Title) {"/title:`"$Title`""}
			if ($ReadOnly) {'/ro'}
			if ($Hidden) {
				'/set:Panel.Left.Visible=false'
				'/set:Panel.Right.Visible=false'
			}
			else {
				'/set:Panel.Left.Visible=true'
				'/set:Panel.Right.Visible=true'
			}
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

	# invoke command
	if ($env:FAR_START_COMMAND -ne ':') {
		if ($env:FAR_START_ENTER) {
			job {
				$Far.CommandLine.Text = $env:FAR_START_COMMAND
			}
			keys Enter
		}
		else {
			job {
				$Far.InvokeCommand($env:FAR_START_COMMAND)
			}
		}
	}

	# quit
	if ($env:FAR_START_QUIT) {
		job {
			$Far.Quit()
		}
	}
}
