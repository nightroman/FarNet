<#
.Synopsis
	Test FileSystem panel

.Description

_110318_140817

	ps: New-Object PowerShellFar.ItemPanel "\\$env:pc_slave\C$\Users\rkuzmin" | Open-FarPanel
	PathInfo:
		Drive: null
		ProviderPath: "\\\\$env:pc_slave\\C$\\Users\\rkuzmin"
		Path: "Microsoft.PowerShell.Core\\FileSystem::\\\\$env:pc_slave\\C$\\Users\\rkuzmin"

	Thus, take the ProviderPath and keep if it is a network path.
	Else: take the Path to reflect PS specific drives.

	Known:
		case: Path.Length == 0: Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER
		case: Path == @"\": ????? what is this?
#>

# make temp files and start panel
job {
	if (Test-Path "C:\TEMP\Tmp") { Remove-Item "C:\TEMP\Tmp" -Force -Recurse }
	$null = md "C:\TEMP\Tmp"
	$null = 1..3 | New-Item -Path "C:\TEMP\Tmp" -Name { "dir$_" + '`$][' } -ItemType directory
	$null = 1..3 | New-Item -Path "C:\TEMP\Tmp" -Name { "file$_" + '`$][' } -ItemType file
	New-Object PowerShellFar.ItemPanel "C:\TEMP\Tmp" | Open-FarPanel
}

# select d1 and f1 and delete selection
macro 'Keys"Down Ins Down Down Ins"'
job {
	$ff = @(Get-FarItem -Selected)
	Assert-Far $ff.Count -eq 2
	Assert-Far @(
		$ff[0].Name -eq 'dir1`$]['
		$ff[1].Name -eq 'file1`$]['
		Test-Path -LiteralPath 'C:\TEMP\Tmp\dir1`$]['
		Test-Path -LiteralPath 'C:\TEMP\Tmp\file1`$]['
	)
}
keys F8
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text.ToUpper().Contains('TARGET "C:\TEMP\TMP\DIR1`$]["') # v4.0 Target -> target
}
keys y
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text.ToUpper().Contains('TARGET "C:\TEMP\TMP\FILE1`$]["') # v4.0 Target -> target
}
keys y
job {
	Assert-Far -Panels
	Assert-Far @(
		!(Test-Path -LiteralPath 'C:\TEMP\Tmp\dir1`$][')
		!(Test-Path -LiteralPath 'C:\TEMP\Tmp\file1`$][')
	)
}

# create file
keys F7
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'New FileSystem item'
}
macro 'Keys"AltN f i l e 1 ` $ ] [ AltT f i l e AltV Del Enter"'
job {
	Assert-Far -Panels -FileName 'file1`$]['
}

# create directory
keys F7
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'New FileSystem item'
}
macro 'Keys"AltN d i r 1 ` $ ] [ AltT d i r e c t o r y AltV Del Enter"'
job {
	Assert-Far -Panels -FileName 'dir1`$]['
}

# open another panel
keys Tab
job {
	New-Object PowerShellFar.ItemPanel 'C:\TEMP\Tmp\dir1`$][' | Open-FarPanel
}

# select and move items to another panel
macro 'Keys"Tab Down Ins Ins Ins Ins"'
keys F6
job {
	Assert-Far -Dialog
	Assert-Far (
		$Far.Dialog[2].Text.ToUpper().Contains('TARGET "ITEM: C:\TEMP\TMP\DIR2`$][ DESTINATION: C:\TEMP\TMP\DIR1`$][\DIR2`$]["')
	)
}
keys a
job {
	Assert-Far @(
		$Far.Panel.GetFiles().Count -eq 2
		$Far.Panel2.GetFiles().Count -eq 4
	)
}

# go to another panel, then up on dots
macro 'Keys"Tab Enter"'
job {
	Assert-Far -FileName 'dir1`$]['
}

# rename the bad name, or Posh fails at this location
keys ShiftF6
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Rename'
}
macro 'Keys"d i r 1 Enter"'
job {
	Assert-Far -Panels -FileName 'dir1'
}

# step in
keys Enter
# copy all items back
macro 'Keys"Multiply F5"'
job {
	Assert-Far -Dialog
	Assert-Far (
		$Far.Dialog[2].Text.ToUpper().Contains('TARGET "ITEM: C:\TEMP\TMP\DIR1\DIR2`$][ DESTINATION: C:\TEMP\TMP\DIR2`$]["')
	)
}
keys a
job {
	Assert-Far -Panels -Plugin
}

# close another panel
keys Esc
job {
	Assert-Far -Panels -Native
}

# go to TEMP
job {
	$Far.Panel.CurrentDirectory = "C:\TEMP"
}

# back to the 1st panel
keys Tab
# rename file 3 (it must be current)
keys ShiftF6
job {
	Assert-Far -Dialog
	Assert-Far @(
		$Far.Dialog[0].Text -eq 'Rename'
		$Far.Dialog[2].Text -eq 'file3`$]['
	)
}
#! Regression Far 3.0.4760 / 50
macro 'Keys"Right 3"'
job {
	Assert-Far ($Far.Dialog[2].Text -eq 'file3`$][3')
}
keys Enter
job {
	Assert-Far -Panels -FileName 'file3`$][3'
}

# open the member panel, edit the file Attributes, make it Hidden
keys CtrlPgDn
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.MemberPanel])
	Find-FarFile Attributes
}
macro 'Keys"Enter Right , H i d d e n Enter"'
job {
	Assert-Far $Far.Panel.CurrentFile.Description.Contains('Hidden')
}

# go back to the item panel, the file has got hidden
keys CtrlPgUp
job {
	#! v5 -a-h--
	Assert-Far @(
		!$Far.Panel.CurrentFile
		(Get-Item -LiteralPath 'C:\TEMP\Tmp\file3`$][3' -Force).Mode -like '-a-h-*'
	)
}

# step out the tmp folder and delete it
keys CtrlPgUp
job {
	Assert-Far -FileName 'Tmp'
}
keys F8
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text.Contains('C:\TEMP\Tmp has children and the Recurse parameter was not specified.')
}
keys y
job {
	# v4.0 amended text
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[2].Text -like '*Performing *operation "Remove Directory" on Target "C:\TEMP\Tmp".*')
}
macro 'Keys"a"'# v4.0 "y" is not enough, it continues to ask
job {
	Assert-Far -Panels
	Assert-Far @(
		!(Test-Path "C:\TEMP\Tmp")
		$Far.Panel.CurrentFile.Name -ne 'Tmp'
	)
}

# exit panel
keys Esc
