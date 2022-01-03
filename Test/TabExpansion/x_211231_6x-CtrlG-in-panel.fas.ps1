<#
.Synopsis
	Covers $_ in PSF "invoke for each" dialog.

.Description
	_211231_6x
	- PSF code sets and restores this variable
	- TabExpansion2 must not use this variable (!)

 	Console host has issues on completing $_, internal NRE.
 	This is not a big deal, why complete $_ in the console?
#>

job {
	# set initial value to test restoring
	$global:_ = '_211231_6x'

	# open a panel with an object
	[PSCustomObject]@{ MustComplete = 42 } | Out-FarPanel
}

job {
	# navigate
	Find-FarFile 42
}

# open the dialog
keys CtrlG

job {
	# it is the "invoke for each" dialog
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::InputDialog)

	# $_ is set to the 1st panel item
	Assert-Far $_.MustComplete -eq 42
}

# type and complete
keys `$ _ . m u s t Tab`

job {
	# it is completed
	Assert-Far $Far.Dialog[2].Text -eq '$_.MustComplete'
}

# exit dialog, panel
keys Esc Esc

job {
	# $_ is restored to initial
	Assert-Far $_ -eq _211231_6x
}
