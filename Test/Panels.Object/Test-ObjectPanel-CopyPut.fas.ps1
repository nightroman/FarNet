<#
.Synopsis
	_100227_073909 Copy from native and any panel
	_200703_80 Изменилось поведение выделения в панели плагина https://forum.farmanager.com/viewtopic.php?p=161468#p161468
#>

job {
	# open object panel
	Out-FarPanel
}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
}

# go to another panel, far.exe.config
keys Tab
job {
	$Far.Panel.GoToPath("$env:FARHOME\Far.exe.config")
	Assert-Far -FileName 'Far.exe.config'
}

### copy to the object panel from the native panel
macro 'Keys"F5 Enter"'
job {
	# 1 file
	Assert-Far $Far.Panel2.ShownFiles.Count -eq 1

	# select it, mind case
	$Far.Panel2.SelectNames(@('Far.exe.config'))
	Assert-Far $Far.Panel2.SelectedFiles.Count -eq 1
}

job {
	# open item panel
	New-Object PowerShellFar.ItemPanel $Far.CurrentDirectory | Open-FarPanel
}
job {
	# item panel, find Far.exe.config
	Assert-Far ($Far.Panel -is [PowerShellFar.ItemPanel])
	Find-FarFile 'Far.exe.config'
}

### copy to the object panel from the power panel
keys F5
job {
	# 2 files, both selected
	Assert-Far @(
		$Far.Panel2.ShownFiles.Count -eq 2
		$Far.Panel2.SelectedList.Count -eq 1 #_200703_80
	)
}

job {
	# find Far.exe
	Find-FarFile 'Far.exe'
}
keys F5
job {
	# 3 files, 2 selected
	Assert-Far @(
		$Far.Panel2.ShownFiles.Count -eq 3
		$Far.Panel2.SelectedList.Count -eq 1 #_200703_80
	)
}

### exit the source panel
keys Esc
job {
	Assert-Far -Native
}

### back to target, exit it
keys Tab
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
}
keys Esc
