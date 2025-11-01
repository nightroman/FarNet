
$Data.Root = 'C:\TEMP\Test-Open-FarViewer.Relative'
$Data.Name = 'Test-Open-FarViewer.Relative.txt'

job {
	# new test root
	$null = mkdir $Data.Root

	# new test file
	Set-Location $Data.Root
	Set-Content $Data.Name 42

	# open using relative path
	Open-FarViewer $Data.Name
}
job {
	# PSF 5.0.84 opens the existing file from the current PS location.
	# PSF 5.0.83 fails to find the file and opens a new with another path.
	Assert-Far -Viewer
	Assert-Far $__.FileName -eq "$($Data.Root)\$($Data.Name)"
}
macro 'Keys"Esc" -- exit viewer'
job {
	Remove-Item $Data.Name
	Set-Location ..
	Remove-Item $Data.Root
}
