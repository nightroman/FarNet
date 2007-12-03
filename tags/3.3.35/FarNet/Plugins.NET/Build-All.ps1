
## Test Build.bat in sub-folders

param
(
	[switch]$Test
)

$ErrorActionPreference = 'Stop'
Get-ChildItem -Recurse -Include Build.bat | .{ process {
	Set-Location $_.PSParentPath
	.\Build.bat
	if ($LastExitCode) { throw }
	if (!(Test-Path *.dll)) { throw }
	Get-Item *.dll
}} | .{process{
	if ($Test) {
		Remove-Item $_
	}
}}
