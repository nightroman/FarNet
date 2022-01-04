<#
.Synopsis
	Clears PowerShell session resources.
	Author: Roman Kuzmin

.Description
	It removes global variables with empty Description and Option values but
	keeps variables listed in this script $keepVariables.

	In addition, the script clears $Error (unless -KeepError), calls garbage
	collection and gets some statistics.

.Parameter KeepError
		Tells to keep errors in $Error.

.Outputs
	An object with properties:
	-- WorkingSet - the current process working set (KB)
	-- ManagedBefore - before garbage collection (KB)
	-- ManagedAfter - after garbage collection (KB)
	-- ErrorCount - current or removed, see -KeepError
	-- RemovedVariableCount
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
$r = 1 | Select-Object WorkingSet, PrivateMemory, ManagedAfter, ManagedBefore, ErrorCount, RemovedVariableCount
$r.ManagedBefore = [long]([System.GC]::GetTotalMemory($false) / 1kb)

# remove variables
$r.RemovedVariableCount = 0
foreach($_ in Get-Variable * -Scope Global) {
	if ((!$_.Description) -and ($_.Options -eq 0) -and ($keepVariables -notcontains $_.Name)) {
		Remove-Variable $_.Name -Scope Global -ErrorAction Continue -Verbose:$Verbose
		++$r.RemovedVariableCount
	}
}

# clear errors
$r.ErrorCount = $Error.Count
if (!$KeepError) {
	$Error.Clear()
}

# collect garbage
$r.ManagedAfter = [long]([System.GC]::GetTotalMemory($true) / 1kb)

# this process info
$process = Get-Process -Id $PID
$r.WorkingSet = $process.WorkingSet64 / 1kb
$r.PrivateMemory = $process.PrivateMemorySize64 / 1kb
$r
