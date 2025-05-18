
Start-FarTask {
	Get-Variable -Scope 0 | Select-Object Name, Options | Export-Csv z.task.0.csv
	Get-Variable -Scope 1 | Select-Object Name, Options | Export-Csv z.task.1.csv
	Get-Variable | Select-Object Name, Options | Export-Csv z.task.2.csv
	job {
		Get-Variable -Scope 0 | Select-Object Name, Options | Export-Csv z.job.0.csv
		Get-Variable -Scope 1 | Select-Object Name, Options | Export-Csv z.job.1.csv
	}
}
