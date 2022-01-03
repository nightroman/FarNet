<#
.Synopsis
	Panel tests cases
#>

#! do not remove or reorder these tests
function Test.GoToPath.Tools
{
	go 'c:/rom/'
	Assert-Far ($Far.Panel.CurrentDirectory -eq 'c:\rom')

	go '\'
	Assert-Far ($Far.Panel.CurrentDirectory -eq 'c:\')

	go '\rom/'
	Assert-Far ($Far.Panel.CurrentDirectory -eq 'c:\rom')

	go '/'
	Assert-Far ($Far.Panel.CurrentDirectory -eq 'c:\')

	go '/rom\\aps//about.ps1'
	Assert-Far ((Get-FarItem).Name -eq 'about.ps1')

	go '\rom'
	Assert-Far ((Get-FarItem).Name -eq 'rom')
}

function _090116_085532 # Get-FarItem -Selected fails on dots if none is selected
{
	$Far.Panel.CurrentDirectory = 'c:\temp'
	Assert-Far (!(Get-FarItem -Selected))
}

### Main
Test.GoToPath.Tools
_090116_085532
