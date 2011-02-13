
<#
.SYNOPSIS
	Shows Windows Favorites as a menu and invokes items.
	Author: Roman Kuzmin

.DESCRIPTION
	This menu navigates through Favorites folders (submenus) and files. Hotkeys
	are assigned automatically, you can control them only by source item names.
	Proper names simplify navigations not only in this menu but in GUI menus as
	well (e.g. IE Favorites menu).

	You can specify any root folder, e.g. your desktop, start menu, programs.
	Any folder tree which mostly contains *.url or *.lnk files is suitable.

	KEYS AND ACTIONS

	[Enter]
	Opens a folder submenu or invokes a file. In panels only: if a file is a
	shortcut (*.lnk) for existing directory then it is opened in a Far panel.

	[Space]
	It works as [Enter] but the menu is not closed after invoking files.

	[BS]
	Goes back to the parent menu, if any.

	[CtrlEnter]
	In panels only: closes the menu and navigates to the item.
#>

param
(
	[string]
	# Root path for the menu.
	$Root = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Favorites)
	,
	[switch]
	# Tells to show folder items recursively.
	$Flat
)

$path = [System.IO.Path]::GetFullPath($Root)
$path0 = $path
$goto = ''

for(;;) {
	### new menu
	$menu = $Far.CreateMenu()
	$menu.Title = Split-Path $path -Leaf
	$menu.AutoAssignHotkeys = $true
	$menu.ShowAmpersands = $true
	$menu.WrapCursor = $true
	$menu.BreakKeys.Add([FarNet.VKeyCode]::Backspace)
	$menu.BreakKeys.Add([FarNet.VKeyCode]::Spacebar)
	if ($Far.Window.Kind -eq 'Panels') {
		$menu.BreakKeys.Add([FarNet.VKeyCode]::Enter -bor [FarNet.VKeyMode]::Ctrl)
	}

	### add items
	$separator = 1
	Get-ChildItem -LiteralPath $path -Recurse:$Flat | .{process{
		if ($separator -and !$_.PSIsContainer) {
			$separator = 0
			if ($menu.Items.Count) {
				$menu.Items.Add((New-FarItem -IsSeparator))
			}
		}
		if ($_.FullName -eq $goto) {
			$menu.Selected = $menu.Items.Count
		}
		$menu.Items.Add((New-FarItem $_.Name -Data $_))
	}}

	### show menu
	$null = $menu.Show()

	### go back (check this case always)
	if ($menu.BreakKey -eq ([FarNet.VKeyCode]::Backspace)) {
		if ($path -ne $path0) {
			$goto = $path
			$path = Split-Path $path
		}
		continue
	}

	$$ = $menu.SelectedData
	if (!$$) { return }

	### go to the item
	if ($menu.breakKey -eq ([FarNet.VKeyCode]::Enter -bor [FarNet.VKeyMode]::Ctrl)) {
		$Far.Panel.GoToPath($$.FullName)
		return
	}

	### open folder submenu
	if ($$.PSIsContainer) {
		$path = $$.FullName
		continue
	}

	### open directory shortcut
	if ($Far.Window.Kind -eq 'Panels' -and $$.Name -like '*.lnk') {
		$WshShell = New-Object -ComObject WScript.Shell
		$target = $WshShell.CreateShortcut([IO.Path]::GetFullPath($$.FullName)).TargetPath
		if ([System.IO.Directory]::Exists($target)) {
			$Far.Panel.CurrentDirectory = $target
			return
		}
	}

	### invoke the item
	Invoke-Item -LiteralPath $$.FullName
	if ($menu.BreakKey -ne ([FarNet.VKeyCode]::Spacebar)) {
		return
	}
	$goto = $$.FullName
}
