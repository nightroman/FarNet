
# x86 on 64 fails to import
if ([Environment]::Is64BitOperatingSystem -and [intptr]::Size -eq 4) {return}

$exit = job {
	#! If there are jobs then exit, not fail let other tests work
	Import-Module BitsTransfer
	if (Get-BitsTransfer) {
		$global:WarningBitsTransferHasJobs = 1
		return 1
	}

	# select Far.exe.example.ini to transfer to TEMP
	$__.CurrentDirectory = "$env:FARHOME"
	$__.SelectNames(@('Far.exe.example.ini'))
	if (Test-Path 'C:\TEMP\Far.exe.example.ini') { Remove-Item 'C:\TEMP\Far.exe.example.ini' }
	$Far.Panel2.CurrentDirectory = 'C:\TEMP'
}
if ($exit) {exit}
run {
	# start the panel with -Auto
	Panel-BitsTransfer.ps1 -Auto
}
job {
	# it promts to transfer the file from .. to ..
	Assert-Far -Dialog
	Assert-Far $__[2].Text -eq "$env:FARHOME\Far.exe.example.ini"
	Assert-Far $__[4].Text -eq 'C:\TEMP\Far.exe.example.ini'
}
# yes
keys Enter
job {
	# panel has started, select the job
	Assert-Far -Plugin
	Assert-Far $__.Title -eq 'BITS Jobs'
	$__.SelectAt(@(1))
}
# kill the job
keys Del
job {
	# it asks
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq 'Remove selected transfer jobs?'
}
# yes
keys Enter
job {
	# empty panel
	Assert-Far -Panels -Plugin
	Assert-Far $__.GetFiles().Count -eq 0
}
# exit
keys Esc
