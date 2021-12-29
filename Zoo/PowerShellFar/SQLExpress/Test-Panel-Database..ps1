<#
.Synopsis
	Test Panel-Database-.ps1 with MSSQL$SQLEXPRESS
#>

# check service exists
try {
	$null = Get-Service 'MSSQL$SQLEXPRESS'
}
catch {
	Write-Warning 'Missing service MSSQL$SQLEXPRESS, skipping the test.'
	return
}

# ensure service running
$Data.SqlServerService2 = Start-Service2 'MSSQL$SQLEXPRESS'

{
	# open database panel
	Panel-Database-.ps1
}
{
	Assert-Far $Far.Panel.Title -eq 'Databases'
	Find-FarFile master
	Find-FarFile tempdb
	Find-FarFile msdb
}

### database tables

'Keys"Enter" -- open msdb database tables'
{
	Assert-Far $Far.Panel.Title -eq 'msdb Tables'
}
#! hack: if `backupfile` is missing use some other
{
	Find-FarFile backupfile
}
'Keys"Enter" -- open table records'
{
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text -eq 'SELECT * FROM dbo.backupfile'
}
'Keys"Enter" -- open table records'
{
	Assert-Far $Far.Panel.Title -eq 'backupfile'
}
'Keys"Esc" -- exit records'
{
	Assert-Far -FileName backupfile
}
'Keys"Esc" -- exit tables'
{
	Assert-Far -FileName msdb
}

### member panel

'Keys"CtrlPgDn" -- open member panel'
{
	Assert-Far ($Far.Panel -is [PowerShellFar.MemberPanel])
}

'Keys"Esc" -- exit member panel'
{
	Assert-Far -FileName msdb
}

'Keys"Enter" -- open database'
{
	Assert-Far $Far.Panel.Title -eq 'msdb Tables'
}

'Keys"Esc" -- exit table panel'
{
	Assert-Far -FileName msdb
}

'Keys"Esc" -- exit panel'
{
	Assert-Far -Native
	Restore-Service2 $Data.SqlServerService2
}
