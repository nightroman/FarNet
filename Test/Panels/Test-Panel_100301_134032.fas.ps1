<#
.Synopsis
	Fixed CtrlQ "index out of range"

.Description
	In Far2 Panel-Macro-.ps1 was used.
	Test-Explorer.far.ps1 looks similar.
#>

### open the panel
job {
	& $env:PSF\Samples\Tests\Test-Explorer.far.ps1
}
job {
	Assert-Far -Plugin -FileName Flat
}

### open QView for the panel
keys CtrlQ
job {
	Assert-Far ($Far.Panel2.Kind -eq 'QView')
}

### go down a few times, watch the QView
keys Down
job {
	Assert-Far -FileName Tree
	Assert-Far ($Far.Panel2.Kind -eq 'QView')
}
keys Down
job {
	Assert-Far -FileName Path
	Assert-Far ($Far.Panel2.Kind -eq 'QView')
}

### exit QView and the panel
keys CtrlQ
job {
	Assert-Far ($Far.Panel2.Kind -ne 'QView')
}
keys Esc
