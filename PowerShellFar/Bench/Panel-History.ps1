<#
.Synopsis
	Far Manager history browser.
	Author: Roman Kuzmin

.Description
	Requires the package FarNet.SQLite.

	This command shows the list of known Far Manager histories:

		Commands
		Folders
		Viewer
		Editor
		Ext.
		Dialog + list of keys

	Select one of them in order to open records in the panel.
	Use F1-menu for Sort and Filter expressions, if needed.
	Use Enter to view the details: name, time, file, data.

	NOTE:
	You can delete selected records by default.
	Use ReadOnly to avoid accidental changes.

.Parameter Database
		Specifies the database path. If it is omitted then the current Far
		Manager history database is opened. The database is read only by
		default.

.Parameter ReadOnly
		Tells to open the database as read only.
#>

[CmdletBinding()]
param(
	[string]$Database = "$env:FARLOCALPROFILE\history.db"
	,
	[switch]$ReadOnly
)

Import-Module $env:FARHOME\FarNet\Lib\FarNet.SQLite
Open-SQLite $Database -ReadOnly:$ReadOnly

$MetaMap = @{
	'0/0' = @{Name='0/0 Commands'; Columns=@('name', 'data')} # name=command; data=folder
	'1/0' = @{Name='1/0 Folders'; Columns=@('name', 'guid', 'file')} # name=folder|null; guid=null|plugin; file=null|plugin-file
	'2/0' = @{Name='2/0 Viewer'; Columns=@('name')} # name=file
	'2/1' = @{Name='2/1 Editor'; Columns=@('name')} # name=file
	'2/2' = @{Name='2/2 Ext.'; Columns=@('name')} # external editor and viewer; name ~ @wordpad "..."
	#'2/3' missing?
	#'2/4' some/what files?
	'3/0' = @{Name='3/0 Dialog'; Columns=@('name')} # key=history-name; name=text
}

# Gets known or unknown meta data.
function Get-Meta($Key) {
	if ($_ = $MetaMap[$Key]) {
		$_
	}
	else {
		@{Name=$Key; Columns=@('name', 'time')}
	}
}

### type/kind
$dt = Get-SQLite 'select distinct kind, type from history order by kind, type'
$it1 = $dt | Out-FarList -Title Kind/Type -Text { (Get-Meta "$($_.kind)/$($_.type)").Name }
if (!$it1) {
	return
}
$Meta = Get-Meta "$($it1.kind)/$($it1.type)"

### key
$dt = Get-SQLite -Table -Parameters @{kind=$it1.kind; type=$it1.type} <#sql#>@'
select key, count() as count
from history
where kind=@kind and type=@type
group by key
order by key
'@
if ($dt.Rows.Count -ge 2) {
	$it2 = $dt | Out-FarList -Title $Meta.Name -Text { "$($_.key) ($($_.count))" }
	if (!$it2) {
		return
	}
}
else {
	$it2 = $dt.Rows[0]
}

### data
$dt = Get-SQLite -Table -Parameters @{kind = $it1.kind; type = $it1.type; key = $it2.key} <#sql#>@'
select id, name, cast(time as text) as time, guid, file, data
from history
where kind=@kind and type=@type and key=@key
order by time desc
'@

# convert time column
foreach($_ in $dt.Rows) {
	$_.time = [datetime]::FromFileTime($_.time)
}
$dt.AcceptChanges()

### panel
$Panel = [PowerShellFar.DataPanel]::new()
$Panel.Explorer.Functions = 'DeleteFiles, GetContent'
$Panel.Garbage.Add($db)
$Panel.Data.db = $db
$Panel.Table = $dt
$Panel.Columns = $Meta.Columns
$Panel.ExcludeMemberPattern = 'id'

$Panel.Title = $Meta.Name
if ($it2.key) {
	$Panel.Title += ' / ' + $it2.key
}

### save
$Panel.AsSaveData = {
	param($Panel)

	# ensure $db
	$db = [System.Data.SQLite.DB]$Panel.Data.db

	# delete and accept each
	foreach($row in $Panel.Table.Select($null, $null, 'Deleted')) {
		Set-SQLite 'delete from history where id = ?' ($row['id', 'Original'])
		$row.AcceptChanges()
	}

	# undo other changes
	$Panel.Table.RejectChanges()
	$true
}

$Panel.Open()
