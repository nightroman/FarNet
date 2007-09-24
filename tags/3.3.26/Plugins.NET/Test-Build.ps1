
## Test all Build.bat in sub-folders

$ErrorActionPreference = 'Stop'
Get-ChildItem -Recurse -Include Build.bat | .{ process {
	Set-Location $_.PSParentPath
	.\Build.bat
	if ($LastExitCode) { throw }
	if (!(Test-Path *.dll)) { throw }
	Get-Item *.dll
}} | Remove-Item -Confirm
