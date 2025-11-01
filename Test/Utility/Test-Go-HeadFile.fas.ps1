
job {
	$__.CurrentDirectory = 'C:\ROM'
	$__.Redraw(0, $true)
	Assert-Far $__.CurrentIndex -eq 0
}
job {
	Go-HeadFile.ps1
	$Data.Index = $__.CurrentIndex
	Assert-Far @(
		$Data.Index -ne 0
		!$__.CurrentFile.IsDirectory
	)
}
keys Up
job {
	Assert-Far $__.CurrentFile.IsDirectory
}
job {
	Go-HeadFile.ps1
	Assert-Far $__.CurrentIndex -eq $Data.Index
}
keys End
job {
	Go-HeadFile.ps1
	Assert-Far $__.CurrentIndex -eq $Data.Index
}
