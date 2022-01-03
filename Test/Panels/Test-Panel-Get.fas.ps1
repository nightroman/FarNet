<#
.Synopsis
	Test cases
#>

job {
	# go to
	$Data.Dir = "$env:FarNetCode\Test\Panels"
	$Far.Panel.CurrentDirectory = $Data.Dir
}

# go to and select a file
macro 'Keys"Alt* P a n e l - G e t Esc Ins Up"'
job {
	Assert-Far ((Get-FarPath) -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far ((Get-FarItem).FullName -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far (@(Get-FarPath -Selected)[0] -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far (@(Get-FarItem -Selected)[0].FullName -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
}

job {
	#! fixed
	$nItem = @(Get-FarItem -All).Count
	$nPath = @(Get-FarPath -All).Count
	$nFile = $Far.Panel.ShownList.Count
	Assert-Far @(
		$nItem -ge 2
		$nItem -eq $nPath
		$nItem -eq $nFile - 1
	)
}

# go to panel 2, open temp panel, copy selection there
macro 'Keys"Tab F11 t Multiply F7"'
macro 'Keys"Tab F5 Enter Tab"'
job {
	Assert-Far $Far.Panel.RealNames
}

# go to file
macro 'Keys"Alt. * P a n e l - G e t Esc"'
job {
	# test temp panel
	Assert-Far ((Get-FarPath) -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far ((Get-FarItem).FullName -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far (@(Get-FarPath -All)[0] -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far (@(Get-FarItem -All)[0].FullName -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far (@(Get-FarPath -Selected)[0] -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
	Assert-Far (@(Get-FarItem -Selected)[0].FullName -eq "$($Data.Dir)\Test-Panel-Get.fas.ps1")
}

# go to folder of the temp panel item
macro 'Keys"CtrlPgUp Tab"'
