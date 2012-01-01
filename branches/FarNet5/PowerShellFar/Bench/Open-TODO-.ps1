
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

	Example of Far Manager file association:
	- Mask: *TODO.xml
	- Command: >: Open-TODO- (Get-FarPath) #
#>

param
(
	# XML file path. Default: TODO.xml in the current directory.
	$Path = ([Environment]::CurrentDirectory + '\TODO.xml')
)

### Make a new table and write to the file
if (![IO.File]::Exists($Path)) {
	# new table
	$t = [System.Data.DataTable]'TODO'

	# Name: TODO name
	$null = $t.Columns.Add('Name')

	# Text: TODO text
	$null = $t.Columns.Add('Text')

	# Rank: to be used to sort
	# 'Attribute': save as XML attribute
	$c = $t.Columns.Add('Rank', [long])
	$c.ColumnMapping = 'Attribute'

	# Date: to be used to sort
	# 'Attribute': save as XML attribute
	$c = $t.Columns.Add('Date', [datetime])
	$c.ColumnMapping = 'Attribute'

	# T: calculated column, sign of Text
	# 'Hidden': tells to not save in XML
	$c = $t.Columns.Add('T')
	$c.ColumnMapping = 'Hidden'
	$c.Expression = "iif((Text is null), '', '+')"

	# save the schema
	$t.WriteXml($Path, [Data.XmlWriteMode]::WriteSchema)
}

### Open the data panel with data from the file
$panel = New-Object PowerShellFar.DataPanel
$panel.XmlFile = $Path

# setup columns
$panel.Columns = @(
	@{ Expression = 'T'; Kind = 'Z'; Width = 1 }
	@{ Expression = 'Name'; Kind = 'N' }
	@{ Expression = 'Rank'; Kind = 'S' }
	@{ Expression = 'Date'; Kind = 'DM' }
)

# [F4] - edit the current record text
$panel.AsEditFile = {
	param($0, $_)
	$name, $text = Edit-NameText $_.Data.Name $_.Data.Text 'TODO'
	if ($name -eq $null) { return }

	if ($name) {
		$_.Data.Name = $name
		$_.Data.Text = if ($text) { $text } else { $null }
	}
	else {
		$_.Data.Delete()
	}

	$0.SaveData()
	$0.Update(0)
	$0.Redraw()
}

# exclude calculated column from members
$panel.ExcludeMemberPattern = '^T$'

# sorting
$panel.ViewSort = 'Rank desc, Date desc'

# go
$panel.Open()

<#
.Synopsis
	Edits name and text together.

.Description
	The name is the first not empty line. The text is all the lines after.

	Output. If the text is not saved then the output is null. Otherwise two
	strings are returned: name and text with trimmed ends. Both can be empty.
#>
function global:Edit-NameText
(
	$Name
	,
	$Text
	,
	$Title
)
{
	$FileName = [IO.Path]::GetTempPath() + [guid]::NewGuid() + '.txt'
	[IO.File]::WriteAllText($FileName, "$Name`n$Text", [Text.Encoding]::Unicode)

	$editor = $Far.CreateEditor()
	$editor.Title = "$Title $Name"
	$editor.FileName = $FileName
	$editor.DisableHistory = $true
	$editor.Open('Modal')

	if ($editor.TimeOfSave -ne [datetime]::MinValue) {
		$lines = [IO.File]::ReadAllLines($FileName, [Text.Encoding]::Unicode)
		for($1 = 0; $1 -lt $lines.Count; ++$1) {
			$Name = $lines[$1].TrimEnd()
			if ($Name) {
				$Name
				break
			}
		}
		if ($1 -lt $lines.Count) {
			($(for(++$1; $1 -lt $lines.Count; ++$1) { $lines[$1].TrimEnd() }) -join "`r`n").TrimEnd()
		}
		else {
			('', '')
		}
	}

	[IO.File]::Delete($FileName)
}
