<#
.Synopsis
	Tests descriptions directly and as tasks.

.Description
	It tests and checks read/write of descriptions then starts a few tasks
	working on descriptions concurrently.

	The test treats descriptions as numbers and increments them on each step.
	In theory with default settings it should complete with description '2100'
	for each file (NTest + NTask * NPass). But in practice this number is less
	due to concurrency.
#>

param(
	# Name X in C:\TEMP\X.
	$TempName = 'Descript.tmp'
	,
	# Count of concurrent task after the direct test.
	$TaskCount = 2
	,
	# Count of description increment passes.
	$PassCount = 1000
)

# recreate test files: 1.tmp .. 9.tmp
$Path = Join-Path 'C:\TEMP' $TempName
if ([System.IO.Directory]::Exists($Path)) {
	Remove-Item "$Path\?.tmp", "$Path\Descript.*" -Force
}
else {
	$null = [System.IO.Directory]::CreateDirectory($Path)
}
foreach($_ in (1..9)) {
	$null > "$Path\$_.tmp"
}

# task code: increment descriptions repeatedly
$script = {
	param($Path, $PassCount)
	Import-Module FarDescription
	for($1 = 0; $1 -lt $PassCount; ++$1) {
		foreach($_ in Get-ChildItem $Path -Filter '?.tmp') {
			$n = [int]$_.FarDescription
			try { $_.FarDescription = [string]($n + 1) }
			catch {}
		}
	}
}

# direct and checked test
& $script $Path 100
foreach($_ in (Get-ChildItem $Path -Filter '?.tmp')) {
	Assert-Far ($_.FarDescription -eq '100')
}

# not checked concurrent tasks
for($1 = 1; $1 -le $TaskCount; ++$1) {
	Start-FarTask -Path $Path -PassCount $PassCount $script
}
