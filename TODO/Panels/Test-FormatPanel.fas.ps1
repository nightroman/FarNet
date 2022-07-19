
job {
	Get-WmiObject Win32_Process -Filter "Name = 'Far.exe'" | Out-FarPanel
}
job {
	$Columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far $Columns.Count -eq $Psf.Settings.MaximumPanelColumnCount
	Assert-Far @(
		$Columns[0].Name -eq 'Name'
		$Columns[1].Name -eq 'Description'
		$Columns[2].Name -eq 'Status'
	)
}
keys Esc
