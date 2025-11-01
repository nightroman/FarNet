
### test: simple output panel
job {
	Get-Process -Id $PID | Out-FarPanel
}
job {
	Assert-Far ($__ -is [PowerShellFar.ObjectPanel])
	Assert-Far $__.GetFiles().Count -eq 1
}
keys CtrlR
job {
	# still 1 file
	Assert-Far $__.GetFiles().Count -eq 1
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
	Assert-Far ($__ -is [PowerShellFar.ObjectPanel])
	Assert-Far $__.GetFiles().Count -eq 1
}
keys CtrlR
job {
	# still 1 file; used to fail
	Assert-Far $__.GetFiles().Count -eq 1
}
keys Esc
