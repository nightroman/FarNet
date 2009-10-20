
<#
.SYNOPSIS
	Clears PowerShell session resources.
	Author: Roman Kuzmin

.DESCRIPTION
	It removes all "unknown" global variables with empty Description, Option
	and Attributes but keeps variables listed in the code as $keep. Thus, you
	may keep your permanent global variables alive by setting one of the
	properties or by adding names to the list.

	In addition, the script clears $Error list (optionally), calls garbage
	collection and returns some statistics.

.OUTPUTS
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
	$KeepError,

	[switch]
	# Writes verbose info.
	$Verbose
)

Set-StrictMode -Version 2

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
		'Culture'
		'DebugPreference'
		'Error'
		'ErrorActionPreference'
		'ErrorView'
		'ExecutionContext'
		'false'
		'FormatEnumerationLimit'
		'HOME'
		'Host'
		'input'
		'LASTEXITCODE'
		'lastWord'
		'line'
		'MaximumAliasCount'
		'MaximumDriveCount'
		'MaximumErrorCount'
		'MaximumFunctionCount'
		'MaximumHistoryCount'
		'MaximumVariableCount'
		'MyInvocation'
		'NestedPromptLevel'
		'null'
		'OutputEncoding'
		'PID'
		'PROFILE'
		'ProgressPreference'
		'PSBoundParameters'
		'PSCmdlet'
		'PSHOME'
		'PSVersionTable'
		'PWD'
		'ReportErrorShowExceptionClass'
		'ReportErrorShowInnerException'
		'ReportErrorShowSource'
		'ReportErrorShowStackTrace'
		'ShellId'
		'StackTrace'
		'this'
		'true'
		'UICulture'
		'VerbosePreference'
		'WarningPreference'
		'WhatIfPreference'
	)

	$r.RemovedVariableCount = 0
	Get-Variable * -Scope Global | .{process{
		if ((!$_.Description) -and ($_.Options -eq 0) -and (!$_.Attributes) -and ($keep -notcontains $_.Name)) {
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
