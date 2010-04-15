
<#
.SYNOPSIS
	Test background jobs.
	Author: Roman Kuzmin

.DESCRIPTION
	This script shows how to use various background jobs, techniques and tests
	some special cases.
#>

###############################################################################
### TEST: PARALLEL JOBS: TWO BACKGROUNG THREADS
# *) shows that parallel jobs may work faster
# *) how to start Far jobs, wait and get results
# *) how to start PowerShell jobs, wait and get results
#
# NOTES: you will see that native PowerShell jobs (Start-Job) look slower.
# Apparently, PowerShell jobs take more time to start, so that they are not
# suitable for small fast tasks. But for slow tasks they are still good and
# they have extra features that Far jobs miss. If value of $count is taken
# large enough then time 3 will be less than time 1.

# loop count
$count = 50000

# task 1
$script1 = {
	$res = 0
	for($e = 0; $e -lt $args[0]; ++$e) { $res += [math]::sin($e) }
	$res
}

# task 2
$script2 = {
	$res = 0
	for($e = 0; $e -lt $args[0]; ++$e) { $res += [math]::cos($e) }
	$res
}

# invoke task scripts one by one
$stopwatch1 = [Diagnostics.Stopwatch]::StartNew()
$res11 = & $script1 $count
$res12 = & $script2 $count
$res1 = $res11 + $res12
$stopwatch1.Stop()

# invoke scripts as parallel Far jobs
$stopwatch2 = [Diagnostics.Stopwatch]::StartNew()
$job1 = Start-FarJob $script1 $count -Output
$job2 = Start-FarJob $script2 $count -Output
# wait for both jobs,
$null = [System.Threading.WaitHandle]::WaitAll(@($job1.Finished, $job2.Finished))
# get the results,
$res21 = $job1.Output[0]
$res22 = $job2.Output[0]
$res2 = $res21 + $res22
# and dispose the jobs!
$job1.Dispose()
$job2.Dispose()
$stopwatch2.Stop()

# invoke scripts as parallel PowerShell jobs
$stopwatch3 = [Diagnostics.Stopwatch]::StartNew()
$job1 = Start-Job $script1 -ArgumentList $count
$job2 = Start-Job $script2 -ArgumentList $count
# wait for both jobs, receive the results,
$res31, $res32 = $job1, $job2 | Wait-Job | Receive-Job
$res3 = $res31 + $res32
# and remove the jobs!
Remove-Job -Job $job1, $job2
$stopwatch3.Stop()

# output results and elapsed times
@"
Results (the same)
result1 : $res1
result2 : $res2
result3 : $res3
Times (normally different)
time1 : $('{0,4} ms' -f $stopwatch1.ElapsedMilliseconds)
time2 : $('{0,4} ms = {1,5:p0}' -f $stopwatch2.ElapsedMilliseconds, ($stopwatch2.ElapsedMilliseconds / $stopwatch1.ElapsedMilliseconds))
time3 : $('{0,4} ms = {1,5:p0}' -f $stopwatch3.ElapsedMilliseconds, ($stopwatch3.ElapsedMilliseconds / $stopwatch1.ElapsedMilliseconds))
"@

###############################################################################
### TEST: PARALLEL JOBS: THIS AND BACKGROUNG THREADS
# This test shows yet another parallel technique: one task is performed in that
# thread and its results immediately processed in this thread.

# start the job in that thread
$job = Start-FarJob { 1..4 } -Output

# process output in this thread
$res = $job.Output | %{ 2 * $_ }
$job.Dispose()

# result contains 4 numbers: 2,?,?,8
Assert-Far @(
	$res.Count -eq 4
	$res[0] -eq 2
	$res[3] -eq 8
)

###############################################################################
### TEST: VIEW OUTPUT IN PROGRESS IN VIEWER
# *) how to use job parameters
# *) how to tell a job to be discarded
# *) look at output in progress in viewer

# job friendly name shown in the job list
$Name = 'Job to view output in progress'

# job command
$Command = {
	# job parameters
	param
	(
		$Title,
		$Items
	)
	# make some output that can be watched in viewer
	"Demo: $Title..."
	foreach($item in $Items) {
		"Demo: $item..."
		Start-Sleep 1
	}
	"Done"
}

# job parameters
$Parameters = @{
	Title = 'Far home directory items'
	Items = (Get-ChildItem -LiteralPath $env:FARHOME)
}

# Discard succeeded job after a few seconds
$KeepSeconds = 15

# start the job with prepared parameters
Start-FarJob -Name:$Name -Command:$Command -Parameters:$Parameters -KeepSeconds:$KeepSeconds

###############################################################################
### TEST: UI JOB WRITE-* CMDLETS AND WRITE-ERROR EFFECT
# *) tests Write-* cmdlets calls in UI job
# *) the job is not discarded even with -KeepSeconds:0 because of errors

# start the job (see Test-Write.ps1, it should be in the same directory)
$script = (Split-Path $MyInvocation.MyCommand.Path) + '\Test-Write.ps1'
Start-FarJob $script -Name 'Test Write-* in job' -KeepSeconds:0

###############################################################################
### TEST: OUTPUT JOB WRITE-* CMDLETS AND WRITE-HOST EFFECT
# *) tests Write-* cmdlets calls in output job
# *) shows that Write-Host is not implemented

# start the job (see Test-Write.ps1, it should be in the same directory)
$script = (Split-Path $MyInvocation.MyCommand.Path) + '\Test-Write.ps1'
$job = Start-FarJob $script -Output

# wait for the job
$null = $job.Finished.WaitOne()

# Output
Assert-Far @(
	$job.Output.Count -eq 1
	$job.Output[0] -eq 'Test of Write-Output'
)

# Error
Assert-Far @(
	$job.Error.Count -eq 3
	$job.Error[0].ToString() -eq 'Cannot invoke this function because the current host does not implement it.'
	$job.Error[1].ToString() -eq 'Test of Write-Error 1'
)

# Debug
Assert-Far @(
	$job.Debug.Count -eq 2
	$job.Debug[0].Message -eq 'Test of Write-Debug 1'
)

# Verbose
Assert-Far @(
	$job.Verbose.Count -eq 2
	$job.Verbose[0].Message -eq 'Test of Write-Verbose 1'
)

# Warning
Assert-Far @(
	$job.Warning.Count -eq 2
	$job.Warning[0].Message -eq 'Test of Write-Warning 1'
)

$job.Dispose()

###############################################################################
### TEST: HIDDEN JOBS AND ERROR EFFECTS
# *) how to start hidden jobs
# *) what happens on hidden job failures
# *) shows that $Far and $Psf are not exposed

# This job works fine and it is discarded together with its output.
Start-FarJob -Hidden {
	Get-Variable Host
	Get-Variable Error
}

# This job finishes with two not terminating errors (variables $Far and $Psf
# are not found). NOTE: default $ErrorActionPreference is 'Continue'
Start-FarJob -Hidden {
	Get-Variable Far
	Get-Variable Psf
}

# This job fails due to a terminating error (variable $Far is not found).
# NOTE: we set $ErrorActionPreference to 'Stop'.
Start-FarJob -Hidden {
	$ErrorActionPreference = 'Stop'
	Get-Variable Far
	Get-Variable Psf
}

###############################################################################
### TEST: JOB RETURNED AND STARTED MANUALLY

# create the job and return without starting
$job = Start-FarJob -Return {
	# data shared between job calls
	++$global:CallCount
	# do some output
	$global:CallCount
	# do some warning
	Write-Warning "Warning $($global:CallCount)"
}

# call the job manually twice and wait for each
$job.StartJob()
$null = $job.Finished.WaitOne()
$job.StartJob()
$null = $job.Finished.WaitOne()

# check the results of both calls
Assert-Far @(
	$job.Output.Count -eq 2
	$job.Output[0] -eq 1
	$job.Output[1] -eq 2
)
Assert-Far @(
	$job.Warning.Count -eq 2
	$job.Warning[0].Message -eq "Warning 1"
	$job.Warning[1].Message -eq "Warning 2"
)
$job.Dispose()
