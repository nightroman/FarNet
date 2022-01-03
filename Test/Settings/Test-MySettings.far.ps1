<#
.Synopsis
	PSF non-module FarNet.ModuleSettings with the specified file.
#>

$script = "$env:FarNetCode\Modules\FarNet.Demo\Scripts\MySettings.far.ps1"
$file = "c:\temp\MySettings.xml"
[IO.File]::Delete($file)

$data1 = & $script
Assert-Far (Test-Path $file)
Assert-Far $data1.Age -eq 1

$data2 = & $script
Assert-Far $data2.Age -eq 2

[IO.File]::Delete($file)
