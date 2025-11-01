<#
.Synopsis
	Panel tests cases
#>

#! do not remove or reorder these tests
function Test.GoToPath.Tools
{
	Go-To 'c:/rom/'
	Assert-Far ($__.CurrentDirectory -eq 'c:\rom')

	Go-To '\'
	Assert-Far ($__.CurrentDirectory -eq 'c:\')

	Go-To '\rom/'
	Assert-Far ($__.CurrentDirectory -eq 'c:\rom')

	Go-To '/'
	Assert-Far ($__.CurrentDirectory -eq 'c:\')

	Go-To '/rom\\aps//about.ps1'
	Assert-Far -FileName about.ps1

	Go-To '\rom'
	Assert-Far -FileName rom

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
