<#
.Synopsis
	Attaches VSCode debugger to Far Manager PowerShell runspace.

.Description
	Prerequisites:
	- Installed VSCode with PowerShell extension.
	- "Start-FarDebug.ps1" is in the path.
	- VSCode launch profile "Attach Far":

		{
		  "type": "PowerShell",
		  "request": "launch",
		  "name": "Attach Far",
		  "script": "Start-FarDebug.ps1"
		}

	How to attach VSCode debugger and hit script breakpoints:
	- Open VSCode (elevated, if Far Manager is elevated).
	- Open required scripts, set breakpoints (F9).
	- Set launch profile to "Attach Far".
	- Start VSCode debugger (F5).
	- Run scripts in Far Manager.

.Parameter Runspace
		Specifies either runspace identifier ([int]) or name ([string]).
		Default: 1 ("main" runspace)

		Use "task" to attach to the runspace created by Start-FarTask.

		Use 0 or '' to choose a runspace interactively in VSCode.
#>

[CmdletBinding()]
param(
	[object]$Runspace = 1
)

Set-StrictMode -Version 3
$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}
if ($Host.Name -ne 'Visual Studio Code Host') {throw 'Requires Visual Studio Code Host.'}

$_processes = @(Get-Process Far -ErrorAction Ignore)
if (!$_processes) {
	Write-Host "Far Manager is not running."
	exit 1
}
if ($_processes.Count -eq 1) {
	$process = $_processes[0]
}
else {
	$prompt = $(for($$ = 0; $$ -lt $_processes.Count; ++$$) {
		"$($$ + 1). $($_processes[$$].Id) $($_processes[$$].MainWindowTitle)"
	}) -join "`n"

	$r = $Host.UI.PromptForChoice('Choose Far Manager process', $prompt, @(
		for($$ = 0; $$ -lt $_processes.Count; ++$$) {
			[System.Management.Automation.Host.ChoiceDescription]::new("$($$ + 1)", '')
		}
	), 0)

	$process = $_processes[$r]
}

if (!$Runspace) {
	Start-DebugAttachSession -ProcessId $process.Id
}
elseif ($Runspace -is [int]) {
	Start-DebugAttachSession -ProcessId $process.Id -RunspaceId $Runspace
}
elseif ($Runspace -is [string]) {
	Start-DebugAttachSession -ProcessId $process.Id -RunspaceName $Runspace
}
else {
	throw
}

$process | Wait-Process
