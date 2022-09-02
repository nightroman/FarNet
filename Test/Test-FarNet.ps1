<#
.Synopsis
	Starts FarNet tests.

.Description
	Examples
		ps: Test-FarNet.ps1
		ps: Test-FarNet.ps1 *
		ps: Test-FarNet.ps1 -All
		ps: Test-FarNet.ps1 .\Test\X\*
		ps: Test-FarNet.ps1 (ls -rec *panel*.ps1)

.Parameter Tests
		| String -> Get-ChildItem path/pattern
		| Some -> test file items
		| -> default tests

.Parameter All
		Tells to invoke extra tests.

.Parameter Quit
		Tells to exit Far after successful tests.
		Or you can set $env:QuitFarAfterTests = 1
#>

[CmdletBinding()]
param(
	$Tests = -1,
	$ExpectedTaskCount = 193,
	$ExpectedBasicsCount = 18,
	$ExpectedExtrasCount = 3,
	[switch]$All,
	[switch]$Quit
)

Assert-Far $env:FarNetCode -Message 'Please set env:FarNetCode'
Assert-Far ($Far.Window.Count -eq 2) -Message 'Please exit editors, viewers, dialogs.'

$global:Error.Clear()
if ($Quit) {$env:QuitFarAfterTests = 1}
$SavedPanelPaths = $Far.Panel.CurrentDirectory, $Far.Panel2.CurrentDirectory

### Resolve $Tests
if ($Tests -is [string]) {
	# path/pattern
	$Tests = Get-ChildItem $Test -Recurse -Include *.fas.ps1
	if (!$Tests) {throw "Found no tests."}
}
elseif (-1 -ne $Tests) {
	# anything but default is items
	Assert-Far ($null -ne $Tests)
}
else {
	# actual default
	$Tests = $null
}

### Initialize
$null = & $PSScriptRoot\About\Initialize-Test.far.ps1
[Diagnostics.Trace]::WriteLine("$(Get-Date) Begin tests")

### Basic tests
if (!$Tests) {
	$basics = @(Get-ChildItem "$env:FarNetCode\Test\Basics" -Filter *.far.ps1)
	Assert-Far $basics.Count -eq $ExpectedBasicsCount
	foreach($test in $basics) {
		[Diagnostics.Trace]::TraceInformation($test.FullName)
		& $test.FullName
		if ($global:Error) {throw "Errors after $($test.FullName)" }
	}
}

### Extra tests
if ($All) {
	$extras = @(
		Get-Item "$env:FarNetCode\Test\TabExpansion\Test-TabExpansion2-.ps1"
		{ Invoke-Build test "$env:FarNetCode\FSharpFar\.build.ps1" }
		{ Invoke-Build test "$env:FarNetCode\JavaScriptFar\.build.ps1" }
	)
	Assert-Far $extras.Count -eq $ExpectedExtrasCount
	foreach($test in $extras) {
		[Diagnostics.Trace]::TraceInformation($test)
		& $test
		if ($global:Error) {throw "Errors after extra test: $test" }
	}
}

### Main tests
Start-FarTask -Data Tests, ExpectedTaskCount, All, SavedPanelPaths {
	$Data.Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
	$Data.TaskCount = 0

	### Collect tests
	if (!$Data.Tests) {
		$Data.Tests = @(
			# tests
			Get-ChildItem $env:FarNetCode\Test -Force -Recurse -Include *.fas.ps1
			# outer
			Get-Item $env:FarNetCode\PowerShellFar\Samples\FarTask\Basics.fas.ps1
			Get-Item $env:FarNetCode\PowerShellFar\Samples\FarTask\Test-Dialog.fas.ps1
		)
	}

	### Show panels
	job {
		$Far.Panel.IsVisible = $true
		$Far.Panel2.IsVisible = $true
	}

	### Run tests
	foreach($test in $Data.Tests) {
		[Diagnostics.Trace]::TraceInformation($test.FullName)

		### Run current test
		$result = job -Arguments $test.FullName {
			switch -Wildcard ($args[0]) {
				*.fas.ps1 {
					++$Data.taskCount
					Start-FarTask $_ -AsTask
					break
				}
				default {
					throw "Unknown test file: $_"
				}
			}
		}

		### Check test output
		if ($result) {
			throw "Unexpected test output:`r`n$($test.FullName)`r`n$result"
		}

		### Check after test
		job {
			try {
				[FarNet.Works.Test]::AssertNormalState()
				if ($global:Error) {throw "Unexpected recorded error: $($global:Error[-1])"}
			}
			catch {
				throw "$($test.FullName): $_"
			}
		}
	}

	### Finish
	ps: {
		[Diagnostics.Trace]::WriteLine("$(Get-Date) End tests")
		$r = Clear-Session -KeepError -Verbose
		$r | Format-List
		if ($r.RemovedVariableCount) {
			Write-Host "Variables: $($r.RemovedVariableCount)" -ForegroundColor Yellow
		}
	}
	job {
		### Quit after tests?
		if ($env:QuitFarAfterTests -eq 1) {
			# clean jobs
			while($job = @([PowerShellFar.Job]::Jobs)) {
				$job[0].Dispose()
			}
			$Far.Quit()
		}
		else {
			### Restore panels
			$Far.Panel.CurrentDirectory = $Data.SavedPanelPaths[0]
			$Far.Panel2.CurrentDirectory = $Data.SavedPanelPaths[1]

			### Start job tests
			if ($Data.All) {
				Start-FarJob { & "$env:PSF\Samples\Tests\Test-Job-.ps1" }
			}

			### Write summary
			$colors = 'Yellow', 'Green'
			Write-Host "Tasks: $($Data.TaskCount)/$($Data.ExpectedTaskCount)" -ForegroundColor ($colors[$Data.TaskCount -eq $Data.ExpectedTaskCount])
			Write-Host "$($Data.Stopwatch.Elapsed)" -ForegroundColor Green
		}
	}
}
