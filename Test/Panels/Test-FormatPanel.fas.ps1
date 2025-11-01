
job {
	$r = [System.Management.ManagementObjectSearcher]::new("SELECT * FROM Win32_Process WHERE ProcessId = $PID").Get()[0]
	$r | Out-FarPanel
}
job {
	$Columns = $__.GetPlan(0).Columns
	Assert-Far $Columns.Count -eq $Psf.Settings.MaximumPanelColumnCount
	Assert-Far @(
		$Columns[0].Name -eq 'Name'
		$Columns[1].Name -eq 'Description'
		$Columns[2].Name -eq 'Status'
	)
	$__.Close()
}
