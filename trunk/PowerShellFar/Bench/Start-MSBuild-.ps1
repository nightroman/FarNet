
<#
.SYNOPSIS
	Starts MSBuild with a helper input dialog for the project file.
	Author: Roman Kuzmin

.DESCRIPTION
	It reads the project file and shows targets, dependencies, and parameters
	with values (these values are usually default but it depends on other
	project conditions, the script does not analyse data).

	If a project file is not specified, the script searches the current
	directory for a single file *.*proj.

	How to use
	Compose [MSBuild options] using for assistance displayed information and
	MSBuild help shown on [Usage]. Then push [Start] to start MSBuild with the
	defined options. It is started in the external console where you take a
	look at the output and then press any key to close the window.

	How to accociate *.*proj files with this script in Far
	In Commands \ File associations insert an entry and set:
	-- Mask: *.*proj
	-- Command: >: Start-MSBuild- (Get-FarPath) #
#>

param
(
	[string]
	# Project file. Default: the only *.*proj file in the current directory.
	$FilePath
)

### resolve the project
if (!$FilePath) {
	$projs = @([IO.Directory]::GetFiles('.', '*.*proj'))
	Assert-Far ($projs.Count -eq 1) "Found $($projs.Count) *.*proj files, use the -FilePath." "Start-MSBuild"
	$FilePath = $projs[0]
}

### get MSBuild path
$msbuild = "$env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
if (![IO.File]::Exists($msbuild)) {
	$msbuild = "$env:windir\Microsoft.NET\Framework\v3.5\MSBuild.exe"
	if (![IO.File]::Exists($msbuild)) {
		$msbuild = [Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory() + 'MSBuild.exe'
	}
}

### import history
$historyPath = Join-Path $Psf.Manager.GetFolderPath('LocalData', $true) Start-MSBuild-.ps1.clixml
if ([System.IO.File]::Exists($historyPath)) {
	$history = Import-Clixml -Path $historyPath
}
else {
	$history = @{}
}

$xml = [xml](Get-Content $FilePath)

$dialog = $Far.CreateDialog(-1, -1, 77, 23)
$null = $dialog.AddBox(3, 1, 0, 0, 'Start MSBuild')

$null = $dialog.AddText(5, -1, 0, 'MSBuild options')
$edit = $dialog.AddEdit(5, -1, 71, '')
$edit.History = '-MSBuild'
$edit.Text = $history[$FilePath]

### target list
$targets = $dialog.AddListBox(5, -1, 71, 7, 'Target : Related targets')
$targets.NoClose = $true
foreach($target in $xml.Project.Target) {
	$text = $target.Name
	if ($target.DependsOnTargets) {
		$text += ' : depends on ' + $target.DependsOnTargets
	}
	if ($target.AfterTargets) {
		$text += ' : after ' + $target.AfterTargets
	}
	if ($target.BeforeTargets) {
		$text += ' : before ' + $target.BeforeTargets
	}
	[void]$targets.Add($text)
}

### properties list
$props = $dialog.AddListBox(5, -$targets.Rect.Height, 71, 7, 'Property = Value')
$props.NoClose = $true
foreach($group in $xml.Project.PropertyGroup) {
	foreach($node in $group.ChildNodes) {
		[void]$props.Add($node.Name + ' = ' + $node.InnerText)
	}
}

$start = $dialog.AddButton(0, $props.Rect.Bottom + 1, '&Start')
$start.CenterGroup = $true
$help = $dialog.AddButton(0, 0, '&Usage')
$help.CenterGroup = $true
$help.NoClose = $true

$help.add_ButtonClicked({
	$text = . msbuild -?
	$Far.AnyViewer.ViewText($text -join "`r", 'MSBuild Help', 'Modal')
})

### show
if (!$dialog.Show()) { return }

### export history, remove missing items
$history[$FilePath] = $edit.Text
foreach($key in @($history.Keys)) {
	if (![System.IO.File]::Exists($key)) {
		$history.Remove($key)
	}
}
Export-Clixml -Path $historyPath -InputObject $history

### run MSBuild
$null = [Diagnostics.Process]::Start('cmd.exe', @"
/c $msbuild "$FilePath" $($edit.Text) & pause
"@)
