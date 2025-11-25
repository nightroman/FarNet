<#
.Synopsis
	Panel tests cases
#>

#! do not remove or reorder these tests
function Test.GoToPath.Tools
{
	Go-To 'c:/temp/'
	Assert-Far $__.CurrentDirectory -eq 'C:\TEMP'

	Go-To '\'
	Assert-Far $__.CurrentDirectory -eq 'C:\'

	Go-To '\temp/'
	Assert-Far $__.CurrentDirectory -eq 'C:\TEMP'

	Go-To '/'
	Assert-Far $__.CurrentDirectory -eq 'C:\'

	Go-To '/temp/RunOnShutdown.log'
	Assert-Far -FileName RunOnShutdown.log

	Go-To '\temp'
	Assert-Far -FileName TEMP

	Go-To Go-To
	Assert-Far -FileName Go-To.ps1
}

function _090116_085532 # Get-FarItem -Selected fails on dots if none is selected
{
	$__.CurrentDirectory = 'c:\temp'
	Assert-Far (!(Get-FarItem -Selected))
}

### Main
Test.GoToPath.Tools
_090116_085532
