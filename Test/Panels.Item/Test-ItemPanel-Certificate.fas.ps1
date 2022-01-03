
job {
	# open cert:
	(New-Object PowerShellFar.ItemPanel cert:\).Open()
}
job {
	Find-FarFile 'CurrentUser'
}
job {
	Find-FarFile 'LocalMachine'
}

# exit panel
keys Esc
