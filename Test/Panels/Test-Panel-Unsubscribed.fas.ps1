
job {
	(New-Object PowerShellFar.PowerExplorer 'f8872f07-f878-4e63-9981-5984c193e620').OpenPanel()
}
job {
	Assert-Far -Panels -Plugin
}
macro 'Keys"F3 F4 F5 F6 F7 F8"'
job {
	Assert-Far -Panels -Plugin
}
job {
	$Far.Panel.Close()
}
