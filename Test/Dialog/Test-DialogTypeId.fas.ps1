﻿<#
.Synopsis
	Tests dialog type ID.

.Link
	_091126_135929
#>

###  Native 'Find' dialog

# open 'Find' dialog
keys AltF7
job {
	# test dialog type ID
	$id = $Far.Dialog.TypeId
	if ($id -ne ([guid]'8C9EAD29-910F-4b24-A669-EDAFBA6ED964')) { throw "TypeId = '$id'" }
}
# close dialog
keys Esc
job {
	# assert panels
	if ($Far.Window.Kind -ne 'Panels') { throw }
}

### FarNet dialog
run {
	# open dialog
	Search-Regex.ps1
}
job {
	# test dialog type ID; _091126_135929
	$id = $Far.Dialog.TypeId
	Assert-Far $id -eq ([guid]'DA462DD5-7767-471E-9FC8-64A227BEE2B1')
}
# close dialog
keys Esc
job {
	Assert-Far -Panels
}
