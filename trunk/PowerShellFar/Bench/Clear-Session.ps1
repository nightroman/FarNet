
<#
.Synopsis
	Clears PowerShell session resources.
	Author: Roman Kuzmin

.Description
	It removes global variables with empty Description and Option values but
	keeps variables listed in the code as $keep.

	In addition, the script clears $Error list (optionally), calls garbage
	collection and gets some statistics.

.Outputs
	An object with properties:
	-- WorkingSet - the current process working set (KB)
	-- ManagedBefore - before garbage collection (KB)
	-- ManagedAfter - after garbage collection (KB)
	-- ErrorCount - current or removed, see -KeepError
	-- RemovedVariableCount
#>

param
(
	[switch]
	# Do not remove errors.
	$KeepError
	,
	[switch]
	# Writes verbose info.
	$Verbose
)

&{
	if ($Verbose) { $VerbosePreference = 'Continue' }

	# stat
	$r = 1 | Select-Object WorkingSet, PrivateMemory, ManagedAfter, ManagedBefore, ErrorCount, RemovedVariableCount
	$r.ManagedBefore = [long]([System.GC]::GetTotalMemory($false) / 1kb)

	# keep these variables:
	$keep = @(
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
		'PSCommandPath' #?? V3
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

	$r.RemovedVariableCount = 0
	Get-Variable * -Scope Global | .{process{
		if ((!$_.Description) -and ($_.Options -eq 0) -and ($keep -notcontains $_.Name)) {
			Remove-Variable $_.Name -Scope Global -ErrorAction Continue -Verbose:$Verbose
			++$r.RemovedVariableCount
		}
	}}

	# now clear errors
	$r.ErrorCount = $Error.Count
	if (!$KeepError) {
		$Error.Clear()
	}

	# garbage collection
	[System.GC]::Collect()
	[System.GC]::WaitForPendingFinalizers()

	# statistics
	$r.ManagedAfter = [long]([System.GC]::GetTotalMemory($false) / 1kb)
	$process = Get-Process -Id $PID
	$r.WorkingSet = $process.WorkingSet64 / 1kb
	$r.PrivateMemory = $process.PrivateMemorySize64 / 1kb
	$r
}
