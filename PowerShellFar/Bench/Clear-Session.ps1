<#
.Synopsis
	Clears PowerShell session resources.
	Author: Roman Kuzmin

.Description
	It removes global variables with empty Description and Option values but
	keeps variables $keepVariables.

	In addition, the script clears $Error (unless -KeepError), calls garbage
	collection and gets some statistics.

.Parameter KeepError
		Tells to keep errors in $Error.

.Outputs
	An object with:
	- WorkingSet - process working set (MB)
	- PrivateMemory - private memory (MB)
	- ManagedMemory - managed memory (MB)
	- ErrorCount - current or removed
	- RemovedVariableCount
#>

[CmdletBinding()]
param(
	[switch]$KeepError
)

if ($MyInvocation.InvocationName -eq '.') {throw 'Do not dot source this script.'}
if ($PSBoundParameters['Verbose']) { $VerbosePreference = 'Continue' }
$ErrorActionPreference=1

# keep these variables
$keepVariables = @(
	'Data' # FarTask
	'r' # REPL
	'Var' # FarTask
	'VSSetupVersionTable' # module VSSetup
	'$'
	'?'
	'^'
	'_'
	'args'
	'ConfirmPreference'
	'ConsoleFileName'
	'DebugPreference'
	'Error'
	'ErrorActionPreference'
	'ErrorView'
	'ExecutionContext'
	'foreach'
	'FormatEnumerationLimit'
	'HOME'
	'Host'
	'input'
	'LastExitCode'
	'LogEngineLifeCycleEvent'
	'LogProviderLifeCycleEvent'
	'Matches'
	'MaximumAliasCount'
	'MaximumDriveCount'
	'MaximumErrorCount'
	'MaximumFunctionCount'
	'MaximumHistoryCount'
	'MaximumVariableCount'
	'MyInvocation'
	'NestedPromptLevel'
	'OFS'
	'OutputEncoding'
	'PID'
	'Profile'
	'ProgressPreference'
	'PSBoundParameters'
	'PSCmdlet'
	'PSCommandPath'
	'PSCulture'
	'PSDebugContext'
	'PSEmailServer'
	'PSHOME'
	'psISE'
	'psScriptRoot'
	'PSSessionApplicationName'
	'PSSessionConfigurationName'
	'PSSessionOption'
	'PSUICulture'
	'PSVersionTable'
	'PWD'
	'ReportErrorShowExceptionClass'
	'ReportErrorShowInnerException'
	'ReportErrorShowSource'
	'ReportErrorShowStackTrace'
	'ShellId'
	'StackTrace'
	'this'
	'VerbosePreference'
	'WarningPreference'
	'WhatIfPreference'
)

# result
$r = 1 | Select-Object WorkingSet, PrivateMemory, ManagedMemory, ErrorCount, RemovedVariableCount

# remove variables
$r.RemovedVariableCount = 0
foreach($_ in Get-Variable * -Scope Global) {
	if ((!$_.Description) -and ($_.Options -eq 0) -and ($keepVariables -notcontains $_.Name)) {
		Remove-Variable $_.Name -Scope Global -ErrorAction Continue -Verbose:$Verbose
		++$r.RemovedVariableCount
	}
}

# clear errors
$r.ErrorCount = $global:Error.Count
if (!$KeepError) {
	$global:Error.Clear()
}

# collect garbage
$r.ManagedMemory = [long]([System.GC]::GetTotalMemory($true) / 1mb)

# process info
$process = Get-Process -Id $PID
$r.WorkingSet = [long]($process.WorkingSet64 / 1mb)
$r.PrivateMemory = [long]($process.PrivateMemorySize64 / 1mb)
$r
