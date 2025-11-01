<#
.Synopsis
	4.3.31: TimeOfOpen, TimeOfSave, Save(), Save(bool)
#>

### editor
job {
	if (Test-Path $HOME/_101104_133707) { Remove-Item $HOME/_101104_133707 }
	Open-FarEditor $HOME/_101104_133707 -DisableHistory
}
job {
	Assert-Far @(
		[Math]::Abs(($__.TimeOfOpen - [DateTime]::Now).TotalSeconds) -lt 3
		$__.TimeOfSave -eq [DateTime]::MinValue
	)
}
job {
	# new file is not saved on soft save
	$__.Save()
	Assert-Far @(
		!(Test-Path $HOME/_101104_133707)
		$__.TimeOfSave -eq [DateTime]::MinValue
	)

	# ditto
	$__.Save($false)
	Assert-Far @(
		!(Test-Path $HOME/_101104_133707)
		$__.TimeOfSave -eq [DateTime]::MinValue
	)

	# hard save does the job
	$__.Save($true)
	Assert-Far @(
		(Test-Path $HOME/_101104_133707)
		[Math]::Abs(($__.TimeOfSave - [DateTime]::Now).TotalSeconds) -lt 3
	)
}
keys Esc
job {
	Assert-Far -Panels
}
### viewer
job {
	Open-FarViewer $HOME/_101104_133707 -DisableHistory
}
job {
	Assert-Far ([Math]::Abs(($__.TimeOfOpen - [DateTime]::Now).TotalSeconds) -lt 3)
}
keys Esc
job {
	Assert-Far -Panels
}
### end
job {
	Remove-Item $HOME/_101104_133707
}
