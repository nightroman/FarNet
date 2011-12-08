
<#
.Synopsis
	Test panel with PowerShell core jobs.
	Author: Roman Kuzmin

.Description
	It creates a few jobs and starts Panel-Job-.ps1. You can view the jobs and
	test panel actions (e.g. [F3], [CtrlQ], [Del], [ShiftDel]).

.Outputs
	Returns new job instances (needed for automated testing).
#>

### New background job
Start-Job -Name "Test job $(Get-Date)" -ScriptBlock {
	'Hello'
	Get-Variable missing
	1..111 | %{
		Start-Sleep 1; "$_ : $(Get-Date)"
	}
}

### New event action job
Register-EngineEvent -SourceIdentifier TestEngineEvent -Action { "Test event: $args" }
$null = New-Event -SourceIdentifier TestEngineEvent -EventArguments 'Hello from event', (Get-Date)

### Open panel
Panel-Job-
