<#
.Synopsis
	Test panel with simple table TestCategories.

.Description
	How to use Panel-DBData to browse a data table.
#>

[CmdletBinding()]
param(
	$Lookup
)

if (!$PSCmdlet.GetVariableValue('DbConnection')) {
	& $PSScriptRoot\Initialize-Test.far.ps1
}

Panel-DBData -Lookup $Lookup -Title TestCategories -TableName TestCategories -Columns Category, Remarks -ExcludeMemberPattern '^CategoryId$'
