
<#
.SYNOPSIS
	Test file decription tools.
	Author: Roman Kuzmin
#>

# make a test directory and a file
$dir = "$env:TEMP\Test-Descript"
$file = "$dir\File 1"
if (Test-Path $dir) {
	Remove-Item $dir\*
}
else {
	$null = New-Item -Path $dir -ItemType Directory
}
$null = New-Item -Path $file -ItemType File

# get the item
$e = Get-Item $file

# set description (use not ASCII text)
$e.FarDescription = 'Test йцукен'
if ($e.FarDescription -ne 'Test йцукен') { throw }
if (!(Test-Path "$dir\Descript.ion")) { throw }

# drop description ($null and '' work the same)
$e.FarDescription = ''
if ($e.FarDescription) { throw }
if (Test-Path "$dir\Descript.ion") { throw }

# end
Remove-Item $dir -Recurse

'Test-Descript- has passed'
