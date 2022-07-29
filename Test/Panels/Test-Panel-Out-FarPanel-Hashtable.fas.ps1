<#
.Synopsis
	Tests object panel with hashtable and dictionary.
#>

# send a hashtable to a panel
job {
	@{ key1 = 'value1'; key2 = 'value2' } | Out-FarPanel
}
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.GetFiles().Count -eq 1
}

# go to the first item - hashtable
macro 'Keys"Alt{ Esc"'
job {
	$CurrentFile = $Far.Panel.CurrentFile
	Assert-Far @(
		$CurrentFile.Name -match '^\{.*\}$'
		$CurrentFile.Description -eq 'System.Collections.Hashtable'
	)
}

# enter the item - hashtable, new object panel is expected with hashtable items
keys Enter
job {
	Assert-Far @(
		$Far.Panel.Title -eq 'Objects'
		$Far.Panel.GetFiles().Count -eq 2
	)
}

# exit all
keys ShiftEsc
job {
	Assert-Far -Native
}

# send a dictionary
job {
	$dic = [System.Collections.Generic.Dictionary[string, object]]::new()
	$dic.Name = 'Joe'
	$dic.Age = 42
	$Data.Dictionary = $dic
	$Data.Dictionary | Out-FarPanel
}
job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.GetFiles().Count -eq 1
}

# go to the first item - dictionary
macro 'Keys"Alt{ Esc"'
job {
	$CurrentFile = $Far.Panel.CurrentFile
	Assert-Far @(
		$CurrentFile.Name -match '^\{\[.*\]\}$'
		$CurrentFile.Description.StartsWith('System.Collections.Generic.Dictionary')
	)
}

# enter the item - dictionary, new object panel is expected with dictionary items
keys Enter
job {
	Assert-Far @(
		$Far.Panel.Title -eq 'Objects'
		$Far.Panel.GetFiles().Count -eq $Data.Dictionary.Count
	)
}

# exit all
keys ShiftEsc
