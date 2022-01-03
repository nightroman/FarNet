<#
.Synopsis
	Test Test-Panel-DbText-.ps1
#>

# skip x64 - there is no Jet
if ([IntPtr]::Size -eq 8) { return }

# check Jet before adding steps, it may be missed on some machines
$DbProviderFactory = [Data.OleDb.OleDbFactory]::Instance
$DbConnection = $DbProviderFactory.CreateConnection()
$DbConnection.ConnectionString = 'Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties="text;hdr=yes;fmt=delimited";Data Source=C:\TEMP'
try { $DbConnection.Open() }
catch { return }

job {
	# open panel
	& "$env:PSF\Samples\Tests\Test-Panel-DbText-.ps1"
}
job {
	Assert-Far -Plugin
}

job {
	# Notepad.exe
	Find-FarFile 'Notepad.exe'
	Assert-Far ($Far.Panel.CurrentFile.Description -match '^\d+$')
}

# exit panel
keys Esc
job {
	Assert-Far -Native
	Remove-Item "$env:TEMP\test[12].csv"
}
