
<#
.Synopsis
	Test Far decription tools.
	Author: Roman Kuzmin

.Description
	The script shows how to get or set Far descriptions and copy, move, rename
	files and directories with their descriptions updated.

	The script works in the $env:TEMP directory, it creates a directory and a
	few files in it. If test is passed all temporary items are removed.
#>

Import-Module FarDescription

### setup: make a test directory and a file in it
$dirPath = "$env:TEMP\Test-Descript"
$filePath = "$dirPath\File 1"
if (Test-Path $dirPath) {
	Remove-Item $dirPath\*
}
else {
	$null = New-Item -Path $dirPath -ItemType Directory
}
$null = New-Item -Path $filePath -ItemType File

### get the directory and file items
# these items have extra members:
# -- property FarDescript (both)
# -- method FarMoveTo() (both)
# -- method FarCopyTo() (file)
$dirItem = Get-Item $dirPath
$fileItem = Get-Item $filePath

### set descriptions
# use not ASCII text, head Alt+160 (to stay), and tail space (to go)
$dirItem.FarDescription = ' Тест описания папки '
$fileItem.FarDescription = ' Тест описания файла '
Assert-Far @(
	Test-Path "$dirPath\Descript.ion"
	Test-Path "$env:TEMP\Descript.ion"
	$dirItem.FarDescription -eq ' Тест описания папки'
	$fileItem.FarDescription -eq ' Тест описания файла'
)

### copy the file with description
$fileItem2 = $fileItem.FarCopyTo("$filePath.txt")
Assert-Far ($fileItem2.FarDescription -eq ' Тест описания файла')

### move (rename) the file with description
$fileItem2.FarMoveTo("$filePath.tmp")
Assert-Far @(
	$fileItem2.Name -eq 'File 1.tmp'
	$fileItem2.FarDescription -eq ' Тест описания файла'
)

### drop the 1st file description; test 2nd file description
$fileItem.FarDescription = ''
Assert-Far @(
	!$fileItem.FarDescription
	Test-Path "$dirPath\Descript.ion"
	$fileItem2.FarDescription -eq ' Тест описания файла'
)

### drop the 2nd file description; Descript.ion is dropped, too
$fileItem2.FarDescription = ''
Assert-Far @(
	!$fileItem2.FarDescription
	!(Test-Path "$dirPath\Descript.ion")
)

### set the 1st description, then delete the file; Descript.ion is created, then dropped
$fileItem.FarDescription = 'Тест удаления с описанием'
Assert-Far @(
	Test-Path "$dirPath\Descript.ion"
	$fileItem.FarDescription -eq 'Тест удаления с описанием'
)
$fileItem.FarDelete()
Assert-Far @(
	!$fileItem.FarDescription
	!(Test-Path "$dirPath\Descript.ion")
)

### move (rename) the directory with description
$dirItem.FarMoveTo("$dirPath.2")
Assert-Far @(
	$dirItem.Name -eq 'Test-Descript.2'
	$dirItem.FarDescription -eq ' Тест описания папки'
)

### drop the directory description
$dirItem.FarDescription = ''
Assert-Far (!$dirItem.FarDescription)

### end
Remove-Item $dirItem.FullName -Recurse
'Test-Descript- has passed'
