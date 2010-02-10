
<#
.SYNOPSIS
	Panel and view/edit Far key macros
	Author: Roman Kuzmin

.DESCRIPTION
	When it is invoked in panels without parameters it opens the customized
	registry panel at KeyMacros drive: you can work with items as usual: [F5],
	[ShiftF5], [F6], [ShiftF6], [F7], [F8] (some operations require another
	panel). [F4] on a macro opens a modal editor to edit a sequence; [Enter]
	opens its view/edit panel, see next remark. To disable a macro without
	removing rename it, e.g. CtrlS to ~CtrlS.

	When it is invoked in panels with -Name and -Area it opens a view/edit
	panel for the specified macro. Use [CtrlS] to save changes and [Esc] to
	exit (you may be asked to save). 'Sequence' and 'Description' are strings
	(use [F4] to edit multi-line sequences). Other data are flags with three
	states: <empty>: DEFAULT; 0: NO; 1: YES.

	When it is invoked in editor or viewer or with -Editor it opens a modal
	editor to edit the macro sequence. You are prompted to enter a macro name
	if it is not defined.

.LINK
	About macros: http://api.farmanager.com/en/macro/index.html

.EXAMPLE
	Panel-Macro-
	Panel-Macro- CtrlS
	Panel-Macro- AltDown Shell
	Panel-Macro- -Path "HKCU:\$($Far.RegistryFarPath)\KeyMacros\Editor\CtrlS"
#>

param
(
	[string]
	# Key name; if none, you may be prompted to enter.
	$Name,

	[string]
	# Macro area name; if none, the current area.
	$Area,

	[string]
	# Full macro registry path.
	$Path,

	[switch]
	# Only edit the sequence.
	$Editor,

	[switch]
	# Show as a child panel.
	$AsChild
)

if ($args) { throw "Unknown parameters: $args" }

# window type, set area if not yet
$wi = $Far.GetWindowInfo(-1, $false)
if (!$Area) {
	switch($wi.Type) {
		'Panels' { $Area = 'Shell'; break }
		'Viewer' { $Area = 'Viewer'; break }
		'Editor' { $Area = 'Editor'; break }
		'Dialog' { $Area = 'Dialog'; break }
		default { throw "Area $($w.Type) is not yet supported." }
	}
}

# global table of macro areas
$global:MacroAreas = @{
	"Common"     = "Common macros with lowest priority"
	"Dialog"     = "Dialog boxes"
	"Disks"      = "Drive selection menu"
	"Editor"     = "Internal file editor"
	"FindFolder" = "Folder search panel"
	"Help"       = "Help system"
	"Info"       = "Informational panel"
	"MainMenu"   = "Main menu"
	"Menu"       = "Other menus"
	"Other"      = "Screen capturing mode"
	"QView"      = "Quick view panel"
	"Search"     = "Quick file search"
	"Shell"      = "File panels"
	"Tree"       = "Folder tree panel"
	"UserMenu"   = "User menu"
	"Viewer"     = "Internal file viewer"
}

### Registry panel configured for macros
if ($wi.Type -eq 'Panels' -and !$Name -and !$Path) {
	$Far.Macro.Save()
	if (!(Test-Path "KeyMacros:\")) {
		$null = New-PSDrive KeyMacros -PSProvider Registry -Root "HKCU:\$($Far.RegistryFarPath)\KeyMacros" -Scope Global
	}
	$p = New-Object PowerShellFar.ItemPanel "KeyMacros:"
	$p.Drive = "KeyMacros"
	$p.Columns = @(
		@{ Label = 'Name'; Expression = 'PSChildName' }
		@{ Label = 'Description'; Expression = { $d = $MacroAreas[$_.PSChildName]; if ($d) { $d } else { $_.GetValue('Description') } } }
	)
	$p.SetOpen({
		if ($this.Panel.Path -match '^KeyMacros:\\(\w+)$') {
			Panel-Macro- -Name $_.File.Name -Area $matches[1] -AsChild
		}
		else {
			$this.OpenFile($_.File)
		}
	})
	$p.SetEdit({
		if ($this.Panel.Path -match '^KeyMacros:\\(\w+)$') {
			Panel-Macro- -Name $_.File.Name -Area $matches[1] -Editor
		}
	})
	$p.add_ItemsChanged({&{
		$Far.Macro.Load()
	}})

	Start-FarPanel $p
	return
}

# parameters
if ($Path) {
	$areaPath = Split-Path $Path
	$Name = Split-Path $Path -Leaf
	$Area = Split-Path $areaPath -Leaf
}
else {
	if (!$Name) {
		$Name = Read-Host "Macro name"
		if (!$Name) { return }
	}
	$areaPath = "HKCU:\$($Far.RegistryFarPath)\KeyMacros\$Area"
	$Path = "$areaPath\$Name"
}

# test name
if ($Far.NameToKey($Name) -eq -1) {
	if ($Far.Message("Key name '$Name' is not valid. Continue anyway?", "Macro", "YesNo")) {
		return
	}
}

# test area
if (!$MacroAreas.Contains($Area)) {
	$Far.Message("Invalid macro area: '$Area'.`rValid values: $($MacroAreas.Keys | Sort-Object)", "Macro")
	return
}

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

### Editor of the sequence
if ($Editor -or $wi.Type -ne 'Panels') {
	# editor loop
	for($s1 = $macro.Sequence;;) {

		# edit in the modal editor
		$s2 = $Far.AnyEditor.EditText($s1, "$Area $Name").TrimEnd()

		# exit with the original, bad or not
		if ($s2 -ceq $macro.Sequence) {
			return
		}

		# check the changed
		if ($Far.Macro.Check($s2, $false) -eq $null) {
			break
		}

		# prompt on errors
		if (1 -eq (Show-FarMessage "The macro sequnce is not valid." -Choices 'Continue changes', 'Discard changes')) {
			return
		}

		# continue with the last text
		$s1 = $s2
	}

	# install and load the changed
	if ($s2 -cne $macro.Sequence) {
		$macro.Sequence = $s2
		$Far.Macro.Install($macro)
		$Far.Macro.Load()
	}

	return
}

### Panel to view/edit a macro
$p = New-Object PowerShellFar.MemberPanel $macro
$p.ExcludeMembers = 'Area', 'Name'
$p.Static = $true

### Saves changes
$p.SetSave({
	$Far.Macro.Install($this.Value)
	$Far.Macro.Load()
	$this.Modified = $false
	[console]::Title = 'Saved'
})

# Go!
Start-FarPanel $p -AsChild -Title ($Area + ' ' + $Name)
