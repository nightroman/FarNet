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
		[Math]::Abs(($Far.Editor.TimeOfOpen - [DateTime]::Now).TotalSeconds) -lt 3
		$Far.Editor.TimeOfSave -eq [DateTime]::MinValue
	)
}
job {
	# new file is not saved on soft save
	$Far.Editor.Save()
	Assert-Far @(
		!(Test-Path $HOME/_101104_133707)
		$Far.Editor.TimeOfSave -eq [DateTime]::MinValue
	)

	# ditto
	$Far.Editor.Save($false)
	Assert-Far @(
		!(Test-Path $HOME/_101104_133707)
		$Far.Editor.TimeOfSave -eq [DateTime]::MinValue
	)

	# hard save does the job
	$Far.Editor.Save($true)
	Assert-Far @(
		(Test-Path $HOME/_101104_133707)
		[Math]::Abs(($Far.Editor.TimeOfSave - [DateTime]::Now).TotalSeconds) -lt 3
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
	Assert-Far ([Math]::Abs(($Far.Viewer.TimeOfOpen - [DateTime]::Now).TotalSeconds) -lt 3)
}
keys Esc
job {
	Assert-Far -Panels
}
### end
job {
	Remove-Item $HOME/_101104_133707
}
