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
	$ExpectedTaskCount = 210,
	$ExpectedBasicsCount = 16,
	$ExpectedExtrasCount = 9,
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
[Diagnostics.Debug]::WriteLine("# $(Get-Date) Begin tests")

### Basic tests
if (!$Tests) {
	$basics = @(Get-ChildItem "$env:FarNetCode\Test\Basics" -Filter *.far.ps1)
	Assert-Far $basics.Count -eq $ExpectedBasicsCount
	foreach($test in $basics) {
		[Diagnostics.Debug]::WriteLine("# $($test.FullName)")
		& $test.FullName
		if ($global:Error) {throw "Errors after $($test.FullName)" }
	}

	#! IB tests after basics
	Invoke-Build ** "$env:FarNetCode\Test\Basics"
}

### Extra tests
$extras = @(
	if ($All) {
		{ Invoke-Build test "$env:FarNetCode\FarNet" }
		Get-Item "$env:FarNetCode\Test\TabExpansion\Test-TabExpansion2-.ps1"
		{ & "$env:FarNetCode\Test\TabExpansion\Test-TabExpansion2.ps1" pwsh }
		{ & "$env:FarNetCode\Test\TabExpansion\Test-TabExpansion2.ps1" powershell }
		{ Invoke-Build test "$env:FarNetCode\GitKit" }
		{ Invoke-Build test "$env:FarNetCode\FSharpFar" }
		{ Invoke-Build test "$env:FarNetCode\JavaScriptFar" }
		{ Invoke-Build test "$env:FarNetCode\JsonKit" }
		{ Invoke-Build test "$env:FarNetCode\RedisKit" }
	}
)
if ($All) {
	Assert-Far $extras.Count -eq $ExpectedExtrasCount
}
foreach($test in $extras) {
	[Diagnostics.Debug]::WriteLine("# $test")
	& $test
	if ($global:Error) {throw "Errors after extra test: $test" }
}

### Main tests
Start-FarTask -Data Tests, ExpectedTaskCount, SavedPanelPaths {
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
	foreach($item in $Data.Tests) {
		++$Data.taskCount
		$TestFile = $item.FullName
		[Diagnostics.Debug]::WriteLine("# $TestFile")

		### Run current test
		$result = job {
			Start-FarTask $Var.TestFile -AsTask
		}

		### Check test output
		if ($result) {
			throw "$TestFile`nUnexpected test output:`n$result"
		}

		### Check test effects
		try {
			job {
				[FarNet.Works.Test]::AssertNormalState()

				if ($global:Error) {
					throw "Unexpected error after test: $($global:Error[-1])"
				}
			}
		}
		catch {
			throw "$TestFile`n$_"
		}
	}

	### Finish
	ps: {
		[Diagnostics.Debug]::WriteLine("# $(Get-Date) End tests")
		$r = Clear-Session -KeepError -Verbose
		$r | Format-List
		if ($r.RemovedVariableCount) {
			Write-Host "Variables: $($r.RemovedVariableCount)" -ForegroundColor Yellow
		}
	}
	job {
		### Quit after tests?
		if ($env:QuitFarAfterTests -eq 1) {
			$Far.Quit()
		}
		else {
			### Restore panels
			$Far.Panel.CurrentDirectory = $Data.SavedPanelPaths[0]
			$Far.Panel2.CurrentDirectory = $Data.SavedPanelPaths[1]

			### Summary
			Write-Host "Tasks: $($Data.TaskCount)/$($Data.ExpectedTaskCount)" -ForegroundColor ($Data.TaskCount -eq $Data.ExpectedTaskCount ? 'Green' : 'Yellow')
			Write-Host "$($Data.Stopwatch.Elapsed)" -ForegroundColor Green

			### DEBUG
			if ((Get-Item $env:FARHOME\FarNet\FarNet.dll).VersionInfo.Comments -like '*DEBUG*') { Write-Host FN=DEBUG -ForegroundColor Red }
			if ((Get-Item $env:FARHOME\FarNet\Modules\PowerShellFar\PowerShellFar.dll).VersionInfo.Comments -like '*DEBUG*') { Write-Host PS=DEBUG -ForegroundColor Red }
		}
	}
}
