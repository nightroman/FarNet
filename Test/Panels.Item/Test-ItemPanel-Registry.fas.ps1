<#
.Synopsis
	Test Registry panel
#>

job {
	# remove hkcu:\a1, open registry panel
	if (Test-Path hkcu:\a1) { Remove-Item hkcu:\a1 -Recurse }
	Go-To hkcu:\
}
job {
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.ItemPanel]
		$Far.Panel.Explorer.Location -eq 'HKCU:\'
	)
}

# new item dialog
keys F7
job {
	Assert-Far -Dialog
}

# enter 'a1'
macro 'Keys"a 1 Enter"'
job {
	Assert-Far -Panels -FileName 'a1'
	Assert-Far $Far.Panel.CurrentFile.IsDirectory
}

# item properties panels
keys CtrlA
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.PropertyPanel])
}

# new propery dialog
keys F7
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'New property'
}

# enter dword value with funny name
macro 'Keys"AltV 1 2 3 AltT d w o r d AltN d w o r d ` $ ] [ * ? Enter"'
job {
	$global:name1 = 'dword`$][*?'
	Assert-Far -FileName $name1
}

# edit it
macro 'Keys"Enter Right 4 5 Enter"'
job {
	Assert-Far (Get-ItemProperty 'hkcu:\a1' *).$name1 -eq 12345
}

# copy here
keys ShiftF5
job {
	Assert-Far -Dialog
	Assert-Far @(
		$Far.Dialog[0].Text -eq 'Copy'
		$Far.Dialog[2].Text -eq 'dword`$][*?'
	)
}
macro 'Keys"Right # Enter"'
job {
	Assert-Far -Panels -FileName "$name1#"
}

# rename
keys ShiftF6
job {
	Assert-Far -Dialog
	Assert-Far @(
		$Far.Dialog[0].Text -eq 'Rename'
		$Far.Dialog[2].Text -eq 'dword`$][*?#'
	)
}
macro 'Keys"Right # Enter"'
job {
	Assert-Far -Panels -FileName "$name1##"
}

# delete
keys F8
job {
	# v4.0 amended text
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like '*Performing *operation "Remove Property" on Target "Item: HKEY_CURRENT_USER\a1 Property: dword*".')
}
keys y
job {
	Assert-Far -FileName $name1
	Remove-Variable name1 -Scope global
}

# new string property
keys F7
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'New property'
}
macro 'Keys"s t r i n g Tab s t r i n g Tab s t r i n g Enter"'
job {
	Assert-Far -Panels -FileName 'string'
}

# edit it
macro 'Keys"Enter Right 2 Enter"'
job {
	Assert-Far (Get-ItemProperty hkcu:\a1 string).string -eq 'string2'
}

# new multistring property
keys F7
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'New property'
}
macro 'Keys"m u l t i s t r i n g Tab m u l t i s t r i n g Tab m u l t i s t r i n g Enter"'
job {
	Assert-Far -Panels -FileName 'multistring'
}

# edit it by F4
keys F4
job {
	Assert-Far -editor
}
macro 'Keys"CtrlEnd l i n e 2 Enter l i n e 3 F2 Esc"'
job {
	$a = (Get-ItemProperty hkcu:\a1 multistring).multistring
	Assert-Far @(
		$a.Count -eq 3
		$a[0] -eq 'multistring'
		$a[1] -eq 'line2'
		$a[2] -eq 'line3'
	)
}

# fix: unwanted dialog about missed file on edit + escape
keys F4
job {
	Assert-Far -Editor
}
keys Esc
job {
	Assert-Far -Panels
}

# go to another panel, create a subkey
keys Tab
job {
	Set-Location hkcu:\a1
	New-Object PowerShellFar.ItemPanel | Open-FarPanel
}
job {
	Assert-Far -Plugin
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.ItemPanel]
		$Far.Panel.Explorer.Location -eq 'HKCU:\a1'
	)
}
macro 'Keys"F7 k e y 1 Enter"'
job {
	Assert-Far -FileName 'key1'
}

# open property panel, dots?
keys CtrlA
job {
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.PropertyPanel]
		!$Far.Panel.CurrentFile
	)
}

# fix: try to copy-here and rename dots
macro 'Keys"ShiftF5 ShiftF6"'
job {
	Assert-Far -Panels
}

# go to panel-1, select and copy all properties, go to panel-2, check
macro 'Keys"Tab Multiply F5 Tab"'
job {
	Assert-Far @(
		$Far.Panel.CurrentDirectory -eq 'hkcu:\a1\key1.*'
		@(Get-FarItem).Count -eq @(Get-FarItem -Passive).Count
	)
}

# select 2 properties and delete
macro 'Keys"Down ShiftDown ShiftDown"'
job {
	$1 = $Far.Panel.SelectedFiles
	Assert-Far @(
		$1.Count -eq 2
		$1[0].Name -eq 'dword`$][*?'
		$1[1].Name -eq 'multistring'
	)
}
keys Del
job {
	# v4.0 amended text
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like '*Performing *operation "Remove Property" on Target "Item: HKEY_CURRENT_USER\a1\key1 Property: dword*".')
}
keys a
job {
	# issues
	Assert-Far @(
		@(Get-FarItem -All).Count -eq 1
		@(Get-FarItem -All -Passive).Count -eq 3
		@(Get-FarPath -All).Count -eq 1
		@(Get-FarPath -All -Passive).Count -eq 3
		@($Far.Panel.ShownFiles).Count -eq 1
		@($Far.Panel2.ShownFiles).Count -eq 3
		@($Far.Panel.ShownItems).Count -eq 1
		@($Far.Panel2.ShownItems).Count -eq 3
	)
}

# select all, move and step out
keys Multiply
job {
	$1 = $Far.Panel.SelectedFiles
	Assert-Far @(
		$1.Count -eq 1
		$1[0].Name -eq 'string'
	)
}
keys F6
job {
	# 'string' has been copied
	Assert-Far @(
		@(Get-FarItem -All).Count -eq 0
		$Far.Panel2.ShownFiles[2].Name -eq 'string'
	)
}
# go up on dots
keys Enter
job {
	Assert-Far -FileName 'key1'
}

# new key
macro 'Keys"F7 AltN k e y 2 Enter"'
job {
	Assert-Far -FileName 'key2'
}

# go to a1 on panel-1
macro 'Keys"Tab CtrlPgUp"'
job {
	Assert-Far -FileName 'a1'
}

# enter a1, go to a1\key1
macro 'Keys"Enter Down"'
job {
	Assert-Far -FileName 'key1'
}

# enter key1
keys Enter
job {
	# issue: must be no files
	Assert-Far $Far.Panel.ShownFiles.Count -eq 0
}

# tab to key2 and copy it to a1\key1
keys Tab
job {
	Assert-Far -FileName 'key2'
}
# copy
keys F5
job {
	# v4.0 amended text
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like 'Performing *operation "Copy Key" on Target "Item: HKEY_CURRENT_USER\a1\key2 Destination: HKEY_CURRENT_USER\a1\key1".')
}

# confirm
keys Enter
job {
	# just copied path
	Assert-Far -Panels
	Assert-Far @(
		Test-Path 'hkcu:\a1\key1\key2'
		$Far.Panel2.ShownFiles[0].Name -eq 'key2'
	)
}

# enter key2
keys Enter
job {
	Assert-Far $Far.Panel.ShownFiles.Count -eq 0
}

# tab, enter dots
macro 'Keys"Tab Enter"'
job {
	Assert-Far (Get-FarItem).Name -eq 'HKEY_CURRENT_USER\a1\key1'
}

# move HKEY_CURRENT_USER\a1\key1 to a1\key2 with children
keys F6
job {
	#! v4.0
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like 'Performing *operation "Move Item" on Target "Item: HKEY_CURRENT_USER\a1\key1 Destination: HKEY_CURRENT_USER\a1\key2".')
}
keys y
job {
	#! v4.0
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like 'Performing *operation "Copy Key" on Target "Item: HKEY_CURRENT_USER\a1\key1 Destination: HKEY_CURRENT_USER\a1\key2".')
}
keys y
job {
	#! v4.0
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like 'Performing *operation "Copy Key" on Target "Item: HKEY_CURRENT_USER\a1\key1\key2 Destination: HKEY_CURRENT_USER\a1\key2\key1\key2".')
}
keys y
job {
	#! V3 RC "Remove key." (with a dot); v4.0
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like 'Performing *operation "Remove Key*" on Target "Item: HKEY_CURRENT_USER\a1\key1".')
}
keys a
job {
	# key2 is current after moving key1
	Assert-Far -Panels
	Assert-Far (Get-FarItem).Name -eq 'HKEY_CURRENT_USER\a1\key2'
}

# go to deep key2 and delete
macro 'Keys"Tab Down Enter Down"'
job {
	Assert-Far (Get-FarItem).Name -eq 'HKEY_CURRENT_USER\a1\key2\key1\key2'
}
keys Del
job {
	#! V3 RC "Remove key." (with a dot); v4.0
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like 'Performing *operation "Remove Key*" on Target "Item: HKEY_CURRENT_USER\a1\key2\key1\key2".')
}
keys y
job {
	Assert-Far -Panels
	Assert-Far @(Get-FarItem -All).Count -eq 0
}

# close panel, go back, close panel
keys Esc
job {
	Assert-Far -Native
}
keys Tab
job {
	Assert-Far -Plugin
}
keys Esc
