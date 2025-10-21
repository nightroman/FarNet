<#
.Synopsis
	TODO notes in XML files.
	Author: Roman Kuzmin

.Description
	This is a toy script but it can be useful, too, as it is or with changes.
	The goal is to show how to use:
	- DataTable with data stored in XML files;
	- DataPanel in order to view and modify that data.

	[F7] adds a new record and opens the panel to edit its fields. Use [Enter]
	and command line to enter one line values or [F4] to edit in the editor.

	[F4] in the table opens the editor to edit Name and Text values together.
	If the result editor text is empty then the record is deleted.

	Far Manager file association:
	- Mask: *TODO.xml
	- Command: vps: Panel-TODO.ps1 (Get-FarPath)

.Parameter Path
		Specifies the file.
		Default is TODO.xml
#>

[CmdletBinding()]
param(
	$Path = 'TODO.xml'
)

#requires -Version 7.4
$ErrorActionPreference=1
trap {$PSCmdlet.ThrowTerminatingError($_)}
if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

$Path = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path)

### Ensure schema file
if (![System.IO.File]::Exists($Path)) {
	# new table
	$t = [System.Data.DataTable]::new('TODO')

	# Name:
	$null = $t.Columns.Add('Name')

	# Text:
	$null = $t.Columns.Add('Text')

	# Rank: to sort, save as attribute
	$c = $t.Columns.Add('Rank', [long])
	$c.ColumnMapping = 'Attribute'

	# Date: to sort, save as attribute
	$c = $t.Columns.Add('Date', [datetime])
	$c.ColumnMapping = 'Attribute'

	# T: calculated column, sign of Text
	# 'Hidden': tells not to save
	$c = $t.Columns.Add('T')
	$c.ColumnMapping = 'Hidden'
	$c.Expression = "iif((Text is null), '', '+')"

	# save schema to file
	$t.WriteXml($Path, [Data.XmlWriteMode]::WriteSchema)
}

### Open panel with file data
$Panel = [PowerShellFar.DataPanel]::new()
$Panel.XmlFile = $Path
$Panel.ExcludeMemberPattern = '^T$'
$Panel.ViewSort = 'Rank desc, Date desc'

# setup columns
$Panel.Columns = @(
	@{ Expression = 'T'; Kind = 'Z'; Width = 1 }
	@{ Expression = 'Name'; Kind = 'N' }
	@{ Expression = 'Rank'; Kind = 'S' }
	@{ Expression = 'Date'; Kind = 'DM' }
)

# [F4] - edit the current record text
$Panel.AsEditFile = {
	param($Panel, $_)
	$name, $text = Edit-NameText $_.Data.Name $_.Data.Text TODO
	if ($null -eq $name) {return}

	if ($name) {
		$_.Data.Name = $name
		$_.Data.Text = $text ? $text : $null
	}
	else {
		$_.Data.Delete()
	}

	$Panel.SaveData()
	$Panel.Update(0)
	$Panel.Redraw()
}

$Panel.Open()

<#
.Synopsis
	Edits name and text together.

.Description
	The name is the first not empty line. The text is all the lines after.

	Output. If the text is not saved then the output is null. Otherwise two
	strings are returned: name and text with trimmed ends. Both can be empty.
#>
function global:Edit-NameText($Name, $Text, $Title) {
	$FileName = [System.IO.Path]::GetTempPath() + [guid]::NewGuid() + '.txt'
	[System.IO.File]::WriteAllText($FileName, "$Name`r`n$Text", [System.Text.Encoding]::Unicode)

	$editor = $Far.CreateEditor()
	$editor.Title = "$Title $Name"
	$editor.FileName = $FileName
	$editor.DisableHistory = $true
	$editor.Open('Modal')

	if ($editor.TimeOfSave -ne [datetime]::MinValue) {
		$lines = [System.IO.File]::ReadAllLines($FileName, [System.Text.Encoding]::Unicode)
		for($$ = 0; $$ -lt $lines.Count; ++$$) {
			$Name = $lines[$$].TrimEnd()
			if ($Name) {
				$Name
				break
			}
		}
		if ($$ -lt $lines.Count) {
			($(for(++$$; $$ -lt $lines.Count; ++$$) {$lines[$$].TrimEnd()}) -join "`r`n").TrimEnd()
		}
		else {
			('', '')
		}
	}

	[System.IO.File]::Delete($FileName)
}
