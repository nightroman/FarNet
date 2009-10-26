
<#
.SYNOPSIS
	Test panel with data from .CSV files
	Author: Roman Kuzmin

.DESCRIPTION
	It shows data from two CSV files using OleDbConnection and Microsoft.Jet
	provider. This demo finds and shows files with the same names in WINDIR and
	WINDIR\SYSTEM32: WINDIR and WINDIR\SYSTEM32 files are exported to temporary
	test1.csv and test2.csv, then test1.csv and test2.csv are joined by SELECT,
	data are shown, files are removed.
#>

# get data to temp files test1.csv and test2.csv
$path = Split-Path $MyInvocation.MyCommand.Path

# open connection (it can fail, e.g. Jet is not installed)
$DbProviderFactory = [Data.OleDb.OleDbFactory]::Instance
$DbConnection = $DbProviderFactory.CreateConnection()
$DbConnection.ConnectionString = @"
Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$path;Extended Properties="text;hdr=yes;fmt=delimited"
"@
try {
	$DbConnection.Open()
}
catch {
	Write-Warning $_
	return
}

# get data now
Get-ChildItem $ENV:WINDIR | Select-Object Name,Length | Export-Csv "$path\test1.csv" -NoTypeInformation
Get-ChildItem $ENV:WINDIR\SYSTEM32 | Select-Object Name,Length | Export-Csv "$path\test2.csv" -NoTypeInformation

# open panel with connection and select statement
Panel-DbData- -CloseConnection -SelectCommand @"
SELECT a.Name, a.Length AS Length1, b.Length AS Length2
FROM test1#csv a INNER JOIN test2#csv b ON a.Name = b.Name
"@

# remove temp files
Remove-Item "$path\test[12].csv"
