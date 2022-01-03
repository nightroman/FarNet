
job {
	#! new global drive if not yet, do not remove, it is "in use"
	if (!(Test-Path FarControlPanel:)) {
		$null = New-PSDrive FarControlPanel -PSProvider Registry -Root 'HKCU:\Control Panel' -Scope global
	}
}

$FixColumns = { ### 'Registry' columns
	job {
		Assert-Far 'SKC VC Name' -eq (($Far.Panel.GetPlan(0).Columns | Select-Object -ExpandProperty Name) -join ' ')
	}
}

job {
	# open panel
	go FarControlPanel:\
}
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.CurrentDirectory -eq 'FarControlPanel:\'
}
& $FixColumns

# enter Desktop
job {
	Find-FarFile Desktop
}
keys Enter
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.CurrentDirectory -eq 'FarControlPanel:\Desktop'
}
& $FixColumns

# ok
keys Esc
