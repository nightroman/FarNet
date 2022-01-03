
job {
	function global:Test-6 ($text) {
		$fc = $host.UI.RawUI.ForegroundColor
		$host.UI.RawUI.ForegroundColor = 'Red'
		$text
		$host.UI.RawUI.ForegroundColor = $fc
	}
}

macro @'
print 'ps: Test-6 Line1; Test-6 Line2'
Keys 'Enter'
'@

<#
ps: T  Gray
Line1  Red
Line2  Red
#>
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = New-Object System.Management.Automation.Host.Rectangle 0, ($y - 3), 4, ($y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv -NoTypeInformation c:\temp\buffer.csv
	$md5 = [guid][System.Security.Cryptography.MD5]::Create().ComputeHash([System.IO.File]::ReadAllBytes('C:\TEMP\buffer.csv'))
	Assert-Far $md5 -eq ([guid]'a23f4856-c53c-5be8-4b65-64270c0c840d')
	Remove-Item C:\TEMP\buffer.csv

	Remove-Item function:\Test-6
}
