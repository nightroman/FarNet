
<#
.SYNOPSIS
	Panel and view/edit Far key macros
	Author: Roman Kuzmin

.DESCRIPTION
	When it is invoked in panels without parameters it opens the customized
	FarMacro provider panel: you can work with items as usual: [F5], [ShiftF5],
	[F6], [ShiftF6], [F7], [F8] (some operations require another panel to be
	opened). [F4] on a macro opens an editor to edit a sequence; [Enter] opens
	its view/edit panel. To disable a macro without removing rename it with '~'
	prefix, e.g. CtrlS to ~CtrlS.

	When it is invoked in panels with -Name and -Area it opens a view/edit
	panel for the specified macro. Use [CtrlS] to save changes and [Esc] to
	exit (you may be asked to save). 'Sequence' and 'Description' are strings
	(use [F4] to edit multi-line sequences). Other data are flags with three
	states: <empty>: DEFAULT; 0: NO; 1: YES.

.LINK
	About macros: http://api.farmanager.com/en/macro/index.html
	FarMacro module
	Edit-FarMacro cmdlet

.EXAMPLE
	Panel-Macro-
	Panel-Macro- CtrlS
	Panel-Macro- AltDown Shell
#>

[CmdletBinding()]
param
(
	# Key name; if none, you may be prompted to enter.
	$Name,

	# Macro area; if none, the current area.
	$Area,

	[switch]
	# Show as a child panel.
	$AsChild
)

Import-Module FarMacro
Set-StrictMode -Version 2

# window type, set area if not yet
$wi = $Far.Window.GetInfoAt(-1, $false)
if (!$Area) {
	switch($wi.Kind) {
		'Panels' { $Area = 'Shell'; break }
		'Viewer' { $Area = 'Viewer'; break }
		'Editor' { $Area = 'Editor'; break }
		'Dialog' { $Area = 'Dialog'; break }
		default { throw "Area $($w.Kind) is not yet supported." }
	}
}
$Area = [FarNet.MacroArea]$Area

### FarMacro panel configured for macros
if ($wi.Kind -eq 'Panels' -and !$Name) {
	$p = New-Object PowerShellFar.ItemPanel "FarMacro:"
	$p.Drive = "FarMacro"
	### [Enter]
	$p.SetOpen({
		if ($this.Panel.Path -match '^FarMacro:\\(\w+)$') {
			Panel-Macro- -Name $_.File.Name -Area $matches[1] -AsChild
		}
		else {
			$this.OpenFile($_.File)
		}
	})
	### [F4]
	$p.SetEdit({
		if ($_.File.Data -is [FarNet.Macro]) {
			Edit-FarMacro -Macro $_.File.Data -Panel $this.Panel
		}
		elseif ($this.Panel.Path -match '\\Consts$') {
			Edit-FarMacro -Area 'Consts' -Name $_.File.Name -Panel $this.Panel
		}
		elseif ($this.Panel.Path -match '\\Vars$') {
			Edit-FarMacro -Area 'Vars' -Name $_.File.Name -Panel $this.Panel
		}
	})
	Start-FarPanel $p
	return
}

# parameters
if (!$Name) {
	$Name = Read-Host "Macro name"
	if (!$Name) { return }
}

# test name
if ($Far.NameToKey($Name.Replace('(Slash)', '/')) -eq -1) {
	if ($Far.Message("Key name '$Name' is not valid. Continue anyway?", "Macro", "YesNo")) {
		return
	}
}
$areaPath = "FarMacro:\$Area\$Name"

# test area path
if (!(Test-Path $areaPath)) {
	$null = New-Item (Split-Path $areaPath) -Name $Area -Confirm
	if (!(Test-Path $areaPath)) {
		return
	}
}

# ready; save current macros, get the macro
$Far.Macro.Save()
$macro = $Far.Macro.GetMacro($Area, $Name)
if (!$macro) {
	$macro = New-Object FarNet.Macro -Property { Area = $Area; Name = $Name }
}

### Panel to view/edit a macro
$p = New-Object PowerShellFar.MemberPanel $macro
$p.ExcludeMemberPattern = '^(Area|Name)$'
$p.Static = $true

### Saves changes
$p.SetSave({
	$Far.Macro.Install($this.Value)
	$Far.Macro.Load()
	$this.Modified = $false
	[console]::Title = 'Saved'
})

# Go!
Start-FarPanel $p -AsChild -Title ("$Area $Name")
