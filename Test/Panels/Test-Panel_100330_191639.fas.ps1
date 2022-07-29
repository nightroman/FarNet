
### test: simple output panel
job {
	Get-Process -Id $PID | Out-FarPanel
}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
	Assert-Far $Far.Panel.GetFiles().Count -eq 1
}
keys CtrlR
job {
	# still 1 file
	Assert-Far $Far.Panel.GetFiles().Count -eq 1
}
keys Esc
job {
	Assert-Far -Native
}

### test: custom output panel
job {
	Get-Process -Id $PID | Out-FarPanel Name, Handles
}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
	Assert-Far $Far.Panel.GetFiles().Count -eq 1
}
keys CtrlR
job {
	# still 1 file; used to fail
	Assert-Far $Far.Panel.GetFiles().Count -eq 1
}
keys Esc
