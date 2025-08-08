<#
.Synopsis
	Starts Far with the command, panels, options.
	Author: Roman Kuzmin

.Description
	This script starts a new Far console with shown or hidden panels with
	optional paths and invokes the specified command in the active panel.

	Use Test or Timeout in order to run FarNet commands as tools or tests.

.Parameter Command
		Specifies the command to be invoked in the active panel.
		By default commands are treated as FarNet commands.
		Use the switch Enter for other commands.

.Parameter Panel1
		Specifies the active panel directory or file.
		Default: the current location.

.Parameter Panel2
		Specifies the passive panel directory or file.
		Default: the Panel1 path.

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
		Tells to wait for exit, check for the exit code, fail if it is not 0.

.Parameter Quit
		Tells to quit Far when the command completes.

.Parameter Test
		Tells to invoke the FarNet command as a tool (synchronous and non
		interactive) and exit after the specified time in milliseconds.
		Exit code: 0 (success) or 1 (failure).

		Use 0 for immediate exit or some positive value in order to pause for
		seeing the command results or errors.

		Test implies ReadOnly, Wait, Enter.

.Parameter Timeout
		Tells to exit if the command runs longer than the specifies time in
		milliseconds. The exit code is set to the timeout value.

		Timeout implies ReadOnly, Wait.

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

$ErrorActionPreference = 1; trap {$PSCmdlet.ThrowTerminatingError($_)}

if ($Command) {
	### Normal call to start Far

	$Panel1 = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Panel1)
	$Panel2 = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Panel2)
	if (!(Test-Path -LiteralPath $Panel1)) {throw "Missing path 1: '$Panel1'."}
	if (!(Test-Path -LiteralPath $Panel2)) {throw "Missing path 2: '$Panel2'."}

	$Environment = if ($Environment) {[hashtable]::new($Environment)} else {@{}}
	$Environment.FARNET_PSF_START_SCRIPT = "Start-FarTask '$($MyInvocation.MyCommand.Path.Replace("'", "''"))'"
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
			"`"$Panel1`""
			"`"$Panel2`""
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
	### Internal call by Start-FarTask

	if (!$env:FAR_START_COMMAND) {
		throw 'Please specify the command or use ":" for void.'
	}

	if ($_ = $env:FAR_START_TEST) {
		[FarNet.Works.Test]::SetTestCommand($_)
	}

	if ($_ = $env:FAR_START_TIMEOUT) {
		[FarNet.Works.Test]::SetTimeout($_)
	}

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

	if ($env:FAR_START_QUIT) {
		job {
			$Far.Quit()
		}
	}
}
