<#
.Synopsis
	Tests descriptions directly and as jobs.

.Description
	It tests and checks read/write of descriptions then starts a few jobs
	working on descriptions concurrently.

	The test treats descriptions as numbers and increments them on each step.
	In theory with default settings it should complete with description '2100'
	for each file (NTest + NJob * NPass). But in practice this number is less
	due to concurrency.

	_090906_154604: jobs and even direct tests may have not terminating errors:
	Exception setting "FarDescription": The requested operation cannot be
	performed on a file with a user-mapped section open.
#>

param
(
	# Name X in C:\TEMP\X.
	$TempName = 'Descript.tmp'
	,
	# Count of concurrent jobs to be started after the direct test.
	$JobCount = 2
	,
	# Count of description increment passes.
	$PassCount = 1000
)

Import-Module FarDescription

# recreate test files: 1.tmp .. 9.tmp
$Path = Join-Path 'C:\TEMP' $TempName
if ([IO.Directory]::Exists($Path)) {
	Remove-Item "$Path\?.tmp", "$Path\Descript.*" -Force
}
else {
	$null = [IO.Directory]::CreateDirectory($Path)
}
foreach($_ in (1..9)) {
	$null > "$Path\$_.tmp"
}

# job code: increment descriptions repeatedly
$script = {
	param
	(
		$Path = $(throw),
		$PassCount
	)
	for($1 = 0; $1 -lt $PassCount; ++$1) {
		foreach($_ in Get-ChildItem $Path -Filter '?.tmp') {
			$n = [int]$_.FarDescription
			$_.FarDescription = [string]($n + 1)
		}
	}
}

# not concurrent checked test
& $script $Path 100
foreach($_ in (Get-ChildItem $Path -Filter '?.tmp')) {
	Assert-Far ($_.FarDescription -eq '100')
}

# start not checked concurrent jobs
for($1 = 1; $1 -le $JobCount; ++$1) {
	Start-FarJob $script @{ Path = $Path; PassCount = $PassCount } -Name:"#$1 Test: $Path" -KeepSeconds:9
}
