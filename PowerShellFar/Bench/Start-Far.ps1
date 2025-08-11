<#
.Synopsis
	Starts Far with the command, panels, options.
	Author: Roman Kuzmin

.Description
	This script starts a new Far console with shown or hidden panels with
	optional paths and invokes the specified command in the active panel.

.Parameter Command
		Specifies the command to be invoked in the active panel.
		By default commands are invoked as FarNet commands.
		Use the switch Enter for other commands.

.Parameter Path
		Specifies the active panel directory or file.
		Default: the current location.

.Parameter Path2
		Specifies the passive panel directory or file.
		Default: the same as Path.

.Parameter Active
		Sets the active panel "Left" or "Right".

.Parameter Title
		Specifies the window title.

.Parameter Exit
		Tells to exit after the command completion with the delay or after
		the timeout, whatever happens first. Specifies one or two numbers:
		delay and timeout in milliseconds. Implies ReadOnly and Wait.

		Exit codes: 0: success, 1: failure, N: timeout value.

.Parameter Environment
		Specifies the environment variables dictionary.

.Parameter WindowStyle
		Specifies the value for Start-Process -WindowStyle.

.Parameter Enter
		Tells to enter the command as typed manually.
		The command may be native, plugin, or FarNet.

.Parameter Hidden
		Tells to start with hidden panels.
		Otherwise both panels are visible.

.Parameter ReadOnly
		Tells to start with read only profile data.

.Parameter Wait
		Tells to wait for exit and fail if the exit code is not 0.

.Example
	># PowerShellFar REPL
	Start-Far 'ps:$Psf.StartCommandConsole()' -Hidden

.Example
	># Far Manager folders
	Start-Far '' $env:FARHOME $env:FARPROFILE -Active Left
#>

[CmdletBinding()]
param(
	[Parameter(Position=0)]
	[string]$Command
	,
	[Parameter(Position=1)]
	[string]$Path = '.'
	,
	[Parameter(Position=2)]
	[string]$Path2
	,
	[ValidateSet('Left', 'Right')]
	[string]$Active
	,
	[string]$Title
	,
	[ValidateCount(1, 2)]
	[int[]]$Exit
	,
	[hashtable]$Environment
	,
	[System.Diagnostics.ProcessWindowStyle]$WindowStyle = 'Normal'
	,
	[switch]$Enter
	,
	[switch]$Hidden
	,
	[switch]$ReadOnly
	,
	[switch]$Wait
)

$ErrorActionPreference = 1; trap {$PSCmdlet.ThrowTerminatingError($_)}

$Path = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path)
if (!(Test-Path -LiteralPath $Path)) {throw "Missing Path: '$Path'."}

if ($Path2) {
	$Path2 = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path2)
	if (!(Test-Path -LiteralPath $Path2)) {throw "Missing Path2: '$Path2'."}
}
else {
	$Path2 = $Path
}

if ($null -eq $Exit) {
	$Delay = $Timeout = -1
}
else {
	$ReadOnly = $Wait = $true
	$Delay = $Exit[0]
	$Timeout = if ($Exit.Length -ge 2) {$Exit[1]} else {-1}
}

$Environment = if ($Environment) {[hashtable]::new($Environment)} else {@{}}
$Environment.FAR_START_COMMAND = if ($Command) {$Command}
$Environment.FAR_START_ENTER = if ($Enter) {1}
$Environment.FAR_START_DELAY = if ($Delay -ge 0) {$Delay}
$Environment.FAR_START_TIMEOUT = if ($Timeout -ge 1) {$Timeout}
$oldEnvironment = @{}
foreach($_ in $Environment.GetEnumerator()) {
	$oldEnvironment.Add($_.Key, [System.Environment]::GetEnvironmentVariable($_.Key))
	[System.Environment]::SetEnvironmentVariable($_.Key, $_.Value)
}

try {
	$exe = if ($env:FARHOME) {"$env:FARHOME\Far.exe"} else {'Far.exe'}
	$arguments = $(
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
		if ($Active) {
			"/set:Panel.LeftFocus=$(if ($Active -eq 'Left') {'true'} else {'false'})"
		}
		"`"$Path`""
		"`"$Path2`""
	)
	$process = Start-Process $exe $arguments -WindowStyle $WindowStyle -PassThru
	if ($Wait) {
		$process.WaitForExit()
		$global:LASTEXITCODE = $process.ExitCode
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
