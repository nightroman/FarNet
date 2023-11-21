<#
.Synopsis
	Scripted FarNet.ModuleSettings with the specified file.
#>

$script = "$env:FarNetCode\Modules\FarNet.Demo\Scripts\MySettings.far.ps1"
$file = "$env:TEMP\MySettings.xml"
[IO.File]::Delete($file)

$data1 = & $script
Assert-Far (Test-Path $file)
Assert-Far $data1.Age -eq 1

$data2 = & $script
Assert-Far $data2.Age -eq 2
