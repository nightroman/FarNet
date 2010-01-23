
<#
.SYNOPSIS
	Test Write-* cmdlets output.
	Author: Roman Kuzmin

.DESCRIPTION
	Some commands are duplicated in order to see how subsequent calls are
	processed internally and looks in formatted output (depends on mode).
#>

Write-Host 'Test of Write-Host'
Write-Output 'Test of Write-Output'

$DebugPreference = 'continue'
Write-Debug 'Test of Write-Debug 1'
Write-Debug 'Test of Write-Debug 2'

$VerbosePreference = 'continue'
Write-Verbose 'Test of Write-Verbose 1'
Write-Verbose 'Test of Write-Verbose 2'

$WarningPreference = 'continue'
Write-Warning 'Test of Write-Warning 1'
Write-Warning 'Test of Write-Warning 2'

$ErrorActionPreference = 'continue'
Write-Error 'Test of Write-Error 1'
Write-Error 'Test of Write-Error 2'
