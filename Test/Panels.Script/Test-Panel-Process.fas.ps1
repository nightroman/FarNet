<#
.Synopsis
	Test Panel-Process.ps1
#>

# start SUT process and open process panel
job {
	$Data.SUT = [System.Diagnostics.Process]::Start('pwsh', '-NoProfile -Command Start-Sleep 9')
	Panel-Process.ps1
}

# navigate to SUT
job {
	Find-FarFile -Where {$_.Data.Id -eq $Data.SUT.Id}
	$ff = @(Get-FarItem -Selected)
	Assert-Far @(
		$ff.Count -eq 1
		$ff[0].ProcessName -eq 'pwsh'
	)
}

# open properties, go to Id
keys CtrlPgDn
job {
	# 2024-11-18-1917 Used to be `Exception getting "CommandLine":...` due to CIM cmdlets problems.
	# In PS 7.5 these problems are really bad, so we just heuristically return null for this.
	Assert-Far $global:Error.Count -eq 0

	Find-FarFile 'Id'
}

# exit properties by Esc
keys Esc
job {
	Assert-Far -FileName 'pwsh'
}

# open WMI properties, go to CommandLine
macro 'Keys"Enter Enter"'
job {
	Find-FarFile 'CommandLine'
}

# exit properties by ..
macro 'Keys"Home Enter"'
job {
	Assert-Far -FileName 'pwsh'
}

# kill it
keys F8
job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq 'Kill selected process(es)?'
}
keys Enter
job {
	Assert-Far $Data.SUT.HasExited
}

# exit
keys Esc
