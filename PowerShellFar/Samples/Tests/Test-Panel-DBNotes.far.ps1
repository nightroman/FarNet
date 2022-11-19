<#
.Synopsis
	Test panel with joined table TestNotes with lookup

.Description
	This script demonstrates some techniques:

	1) Panel uses data adapter with manually configured INSERT, DELETE and
	UPDATE commands. Why?
	- when SELECT joins data from several table automatic command generation
	may not work;
	- automatically generated commands are not optimal and they don't use not
	standard data.

	2) When you <Enter> on the field Category the panel opens another (lookup)
	table TestCategories. You have to select a category by name and press
	[Enter], CategoryId is also taken internally.

	3) SELECT gets more data than shown: e.g. NoteId and CategoryId. A user
	never changes or see them but they are used internally for SQL commands and
	lookup - that is why they are still selected.
#>

[CmdletBinding()]
param(
	[switch]$GenericLookup
)

if (!$PSCmdlet.GetVariableValue('DbConnection')) {
	& $PSScriptRoot\Initialize-Test.far.ps1
}

# data adapter and command to select data
$adapter = $DbProviderFactory.CreateDataAdapter()
$adapter.SelectCommand = $cmd = $DbConnection.CreateCommand()
$cmd.CommandText = @"
SELECT n.Note, c.Category, n.Created, n.NoteId, n.CategoryId
FROM TestNotes n JOIN TestCategories c ON n.CategoryId = c.CategoryId
"@

# reusable script blocks to add command parameters
$NoteId = {
	$prm = $cmd.CreateParameter()
	$prm.ParameterName = $prm.SourceColumn = 'NoteId'
	$prm.DbType = [Data.DbType]::Int32
	$null = $cmd.Parameters.Add($prm)
}
$CategoryId = {
	$prm = $cmd.CreateParameter()
	$prm.ParameterName = $prm.SourceColumn = 'CategoryId'
	$prm.DbType = [Data.DbType]::Int32
	$null = $cmd.Parameters.Add($prm)
}
$Note = {
	$prm = $cmd.CreateParameter()
	$prm.ParameterName = $prm.SourceColumn = 'Note'
	$prm.DbType = [Data.DbType]::String
	$null = $cmd.Parameters.Add($prm)
}
$Created = {
	$prm = $cmd.CreateParameter()
	$prm.ParameterName = $prm.SourceColumn = 'Created'
	$prm.DbType = [Data.DbType]::DateTime
	$null = $cmd.Parameters.Add($prm)
}

### command to insert data
$adapter.InsertCommand = $cmd = $DbConnection.CreateCommand()
$cmd.CommandText = <#sql#>@'
INSERT TestNotes (Note, CategoryId, Created)
VALUES (@Note, @CategoryId, @Created)
'@
. $CategoryId
. $Note
. $Created

### command to delete data
$adapter.DeleteCommand = $cmd = $DbConnection.CreateCommand()
$cmd.CommandText = <#sql#>@'
DELETE FROM TestNotes
WHERE NoteId = @NoteId
'@
. $NoteId

### command to update data
$adapter.UpdateCommand = $cmd = $DbConnection.CreateCommand()
$cmd.CommandText = <#sql#>@'
UPDATE TestNotes
SET Note = @Note, CategoryId = @CategoryId, Created = @Created
WHERE NoteId = @NoteId
'@
. $NoteId
. $CategoryId
. $Note
. $Created

# create and configure panel
$Panel = [PowerShellFar.DataPanel]::new()
$Panel.Adapter = $adapter
$Panel.Title = 'TestNotes'
$Panel.Columns = @(
	@{ Kind = 'N'; Expression = 'Note'; Width = -80 }
	@{ Kind = 'Z'; Expression = 'Category' }
	@{ Kind = 'DC'; Expression = 'Created' }
)
$Panel.ExcludeMemberPattern = '^(NoteId|CategoryId)$'

# Setup lookup taking selected CategoryId (to use) and Category (to show);
# there are two alternative examples below:
#
# 1) GENERIC LOOKUP (NOT SIMPLE BUT MORE UNIVERSAL)
# $0 - event sender, it is a lookup panel created by Test-Panel-DBCategories.far.ps1
# $0.Parent - its parent panel, it is a member panel with TestNotes row fields
# $0.Parent.Value - destination row with CategoryId and Category
# $_ - event arguments (PowerShellFar.FileEventArgs)
# $_.File.Data - lookup row from TestCategories
#
# 2) DATA LOOKUP (SIMPLE BUT ONLY FOR DATA PANELS)
# Actually use this simple way always when possible.
# $0.CreateDataLookup() returns a helper handler that makes all the generic job.

if ($GenericLookup) {
	$Panel.AddLookup('Category', {
		param($0, $_)
		& "$PSScriptRoot\Test-Panel-DBCategories.far.ps1" -Lookup {
			param($0, $_)
			$r1 = $0.Parent.Value
			$r2 = $_.File.Data
			$r1.CategoryId = $r2.CategoryId
			$r1.Category = $r2.Category
		}
	})
}
else {
	$Panel.AddLookup('Category', {
		param($0, $_)
		& "$PSScriptRoot\Test-Panel-DBCategories.far.ps1" -Lookup $0.CreateDataLookup(@('Category', 'Category', 'CategoryId', 'CategoryId'))
	})
}

# go!
$Panel.Open()
