
<#
.SYNOPSIS
	TODO notes in XML files.
	Author: Roman Kuzmin

.DESCRIPTION
	This is a toy script but it can be useful, too, as it is or with changes.
	The goal is to show how to use:
	- DataTable with data stored in XML files;
	- DataPanel in order to view and modify that data.

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

	# Text: TODO subject
	$c = $t.Columns.Add('Text', [string])

	# Rank: to be used to sort
	# 'Attribute': save as XML attribute
	$c = $t.Columns.Add('Rank', [long])
	$c.ColumnMapping = 'Attribute'

	# Date: to be used to sort
	# 'Attribute': save as XML attribute
	$c = $t.Columns.Add('Date', [datetime])
	$c.ColumnMapping = 'Attribute'

	# Memo: TODO description
	$c = $t.Columns.Add('Memo', [string])

	# M: calculated column, sign of Memo
	# 'Hidden': tells to not save in XML
	$c = $t.Columns.Add('M', [string])
	$c.ColumnMapping = 'Hidden'
	$c.Expression = "iif((Memo is null), '', '+')"

	# save the schema now
	$t.WriteXml($Path, [Data.XmlWriteMode]::WriteSchema)
}

### Open the data panel with data imported from the file
$panel = [PowerShellFar.DataPanel]$Path

# setup columns
$panel.Columns = @(
	@{ Expression = 'M'; Kind = 'Z'; Width = 1 }
	@{ Expression = 'Text'; Kind = 'N' }
	@{ Expression = 'Rank'; Kind = 'S' }
	@{ Expression = 'Date'; Kind = 'DC' }
)

# exclude calculated column from members
$panel.ExcludeMemberPattern = '^M$'

# go
$panel.Open()
