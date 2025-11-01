<#
.Synopsis
	Starts FarNet tests.

.Description
	Examples
		ps: Test-FarNet
		ps: Test-FarNet *
		ps: Test-FarNet .\Test\X\*
		ps: Test-FarNet (ls -rec *panel*.ps1)

.Parameter Tests
		| String -> Get-ChildItem path/pattern
		| Some -> test file items
		| -> default tests
#>

[CmdletBinding()]
param(
	$Tests = -1,
	$ExpectedTaskCount = 213,
	$ExpectedIBTestCount = 5,
	$ExpectedBasicsCount = 15
)

Assert-Far $env:FarNetCode -Message 'Requires env:FarNetCode'
Assert-Far ($Far.Window.Count -eq 2) -Message 'Exit editors, viewers, dialogs.'

$env:_branch = '?'
$global:Error.Clear()
$SavedPanelPaths = $__.CurrentDirectory, $Far.Panel2.CurrentDirectory

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
$null = & {
	# ensure expected panel modes
	$panel1 = $__
	$panel2 = $Far.Panel2
	$panel1.SortMode = $panel2.SortMode = 'Name'

	# assert and clear
	[FarNet.Works.Test]::AssertNormalState()
	Clear-Session

	# ensure Mongo for all tests
	if (!$Tests) {
		Start-Mongo.ps1
	}
}
[Diagnostics.Debug]::WriteLine("## $(Get-Date) Begin tests")

### Basic tests first
if (!$Tests) {
	$items = @(Get-ChildItem "$env:FarNetCode\Test\Basics" -Filter *.far.ps1)
	Assert-Far $items.Count -eq $ExpectedBasicsCount
	foreach($test in $items) {
		[Diagnostics.Debug]::WriteLine("## $($test.FullName)")
		& $test.FullName
		Assert-Far -NoError -Message "Errors after $($test.FullName)"
	}
}

### Then IB tests
if (!$Tests) {
	$items = @(Get-ChildItem $env:FarNetCode\Test -Recurse -Include *.test.ps1)
	Assert-Far $items.Count -eq $ExpectedIBTestCount
	Invoke-Build ** $env:FarNetCode\Test
}

### Main tests
[FarNet.Works.ExitManager]::BeginJobs()
Start-FarTask -Data Tests, ExpectedTaskCount, SavedPanelPaths {
	$Data.Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
	$Data.TaskCount = 0

	### Collect tests
	if (!$Data.Tests) {
		$Data.Tests = @(
			# tests
			Get-ChildItem $env:FarNetCode\Test -Force -Recurse -Include *.fas.ps1
			# outer
			Get-Item $env:FarNetCode\Samples\FarTask\Basics.fas.ps1
			Get-Item $env:FarNetCode\Samples\FarTask\Test-Dialog.fas.ps1
		)
	}

	### Show panels
	job {
		$__.IsVisible = $true
		$Far.Panel2.IsVisible = $true
	}

	### Run tests
	foreach($item in $Data.Tests) {
		++$Data.taskCount
		$TestFile = $item.FullName
		[Diagnostics.Debug]::WriteLine("## $TestFile")

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
		[Diagnostics.Debug]::WriteLine("## $(Get-Date) End tests")
		$r = Clear-Session -KeepError -Verbose
		$r | Format-List
		if ($r.RemovedVariableCount) {
			Write-Host "Variables: $($r.RemovedVariableCount)" -ForegroundColor Yellow
		}
	}
	job {
		### Exiting
		if ([FarNet.Works.ExitManager]::IsExiting) {
			[FarNet.Works.ExitManager]::EndJobs()
			return
		}

		### Restore panels
		$__.CurrentDirectory = $Data.SavedPanelPaths[0]
		$Far.Panel2.CurrentDirectory = $Data.SavedPanelPaths[1]

		### Summary
		Write-Host "Tasks: $($Data.TaskCount)/$($Data.ExpectedTaskCount)" -ForegroundColor ($Data.TaskCount -eq $Data.ExpectedTaskCount ? 'Green' : 'Yellow')
		Write-Host "$($Data.Stopwatch.Elapsed)" -ForegroundColor Green

		### DEBUG
		if ((Get-Item $env:FARHOME\FarNet\FarNet.dll).VersionInfo.Comments -like '*DEBUG*') { Write-Host FN=DEBUG -ForegroundColor Red }
		if ((Get-Item $env:FARHOME\FarNet\Modules\PowerShellFar\PowerShellFar.dll).VersionInfo.Comments -like '*DEBUG*') { Write-Host PS=DEBUG -ForegroundColor Red }

		### end
		Set-Content temp:Test-FarNet.end.txt (Get-Date -Format o)
	}
}
