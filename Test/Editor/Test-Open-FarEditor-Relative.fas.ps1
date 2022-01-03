
$Data.Root = 'C:\TEMP\Test-Open-FarEditor.Relative'
$Data.Name = 'Test-Open-FarEditor.Relative.txt'

job {
	# new test root
	$null = mkdir $Data.Root

	# new test file
	Set-Location $Data.Root
	Set-Content $Data.Name 42

	# open using relative path
	Open-FarEditor $Data.Name
}
job {
	# PSF 5.0.84 opens the existing file from the current PS location.
	# PSF 5.0.83 fails to find the file and opens a new with another path.
	Assert-Far -EditorFileName "$($Data.Root)\$($Data.Name)"
}
macro 'Keys"Esc" -- exit editor'
job {
	Remove-Item $Data.Name
	Set-Location ..
	Remove-Item $Data.Root
}
