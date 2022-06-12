<#
.Synopsis
	Test the Alias provider panel

.Description
	_120602_122636 V3 RC introduces silent errors on opening members.
	Case: open Alias panel, go to %, open members, see PS errors:

		* Object reference not set to an instance of an object.
			no comments

		* Exception calling "GetHelpUri" with "1" argument(s): "Nested pipeline should run only from a running pipeline."
			HelpUri getter, script method, fails.
			I can reproduce this only in a panel.
			131221 It's gone if we use InitialSessionState instead of RunspaceConfiguration
#>

job {
	if ($global:Error) {throw 'Please remove errors.'}

	# ensure no aliases
	Remove-Item 'alias:\%[12]'

	# open panel
	Go-To Alias:\
}
job {
	# It's questionable to have a path like this, but why not? But there was a bug, see below
	# _110227_123432 We do not have such paths. Test anyway.
	Assert-Far @(
		$Far.Panel.CurrentDirectory -eq 'Alias:\'
		!(Get-FarFile) # dots
	)
}

# view all
keys F3
job {
	Assert-Far -Viewer
}
keys Esc
job {
	Assert-Far -Panels
}

# go to %
job {
	Find-FarFile '%'
}

# edit + escape
keys F4
job { Assert-Far -Editor }
keys Esc
job { Assert-Far -Panels }

# fix: edit %, exactly this name was problematic
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq 'ForEach-Object'
	$Far.Editor[0].Text = 'ForEach-Object #'
}
macro 'Keys"F2 Esc"'
job {
	# should be updated
	Assert-Far -Panels -FileName '%' -FileDescription 'ForEach-Object #'
}
keys F4
job {
	Assert-Far -Editor
	Assert-Far $Far.Editor[0].Text -eq 'ForEach-Object #'
	$Far.Editor[0].Text = 'ForEach-Object'
}
macro 'Keys"F2 Esc"'
job {
	# should be updated
	Assert-Far -Panels -FileName '%' -FileDescription 'ForEach-Object'
}

# new alias
# _091019_081503 avoid missed alias target, it takes too much memory
macro 'Keys"F7 % 1 AltT Del AltV G e t - H e l p Enter"'
job {
	# columns =
	Assert-Far -FileName '%1' -FileDescription 'Get-Help' -FileOwner ''
	Assert-Far @($Far.Panel.CurrentFile.Columns)[0] -eq 'None'
}

# not supported
keys ShiftEnter
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Operation is not supported by the provider.'
}
keys Esc
job {
	Assert-Far -Panels
}

# view file
keys F3
job {
	Assert-Far -Viewer
}
keys Esc
job {
	Assert-Far -Panels
}

# open members
job {
	# no errors yet
	Assert-Far (!$global:Error)
}

keys CtrlPgDn
job {
	#_120602_122636
	Assert-Far $global:Error.Count -eq 1
	$global:Error.Clear()

	# to Definition
	Find-FarFile 'Definition'
}

# try to edit Definition
# this will show a dialog about read only property
macro 'Keys"Enter N e w V a l u e Enter"'
job {
	Assert-Far -Dialog
}
keys Esc
job {
	# to Description
	Find-FarFile 'Description'
}

# edit Description, then we'll see it is not copied
macro 'Keys"Enter N e w D e s c r i p t i o n Enter"'
job {
	Assert-Far -FileDescription 'NewDescription'

	#_120602_122636
	Assert-Far $global:Error.Count -eq 3
	$global:Error.Clear()
}

# exit members
keys Esc
job {
	Assert-Far -FileName '%1' -FileOwner 'NewDescription'
}

# copy here
macro 'Keys"ShiftF5 % 2 Enter"'
job {
	# columns =
	Assert-Far -FileName '%2' -FileDescription 'Get-Help' -FileOwner ''
	Assert-Far @($Far.Panel.CurrentFile.Columns)[0] -eq 'None'
}

# delete %2
keys Del
job {
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like '*Target "Item: %2"*') # v4.0 Target -> target
}
keys y
job {
	Assert-Far -Panels
	Assert-Far ($Far.Panel.CurrentFile.Name -ne '%2')
}

# find %1, delete
job {
	Find-FarFile '%1'
}
keys Del
job {
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like '*Target "Item: %1"*') # v4.0 Target -> target
}
keys y
job {
	Assert-Far -Panels
	Assert-Far ($Far.Panel.CurrentFile.Name -ne '%1')
}

# exit panel
keys Esc
job {
	# _091019_081503
	Assert-Far -Native
	Assert-Far (!$global:Error)
}
