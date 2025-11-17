
job {
	function global:Test-6 ($text) {
		$fc = $host.UI.RawUI.ForegroundColor
		$host.UI.RawUI.ForegroundColor = 'Red'
		$text
		$host.UI.RawUI.ForegroundColor = $fc
	}
}

macro "print 'ps: Test-6 Line1; Test-6 Line2'; Keys 'Enter'" # $r

<#

Line1  Red
Line2  Red
#>
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = [System.Management.Automation.Host.Rectangle]::new(0, $y - 3, 4, $y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv C:\TEMP\buffer.csv
	$hash = (Get-FileHash C:\TEMP\buffer.csv).Hash
	Assert-Far $hash -eq 377FBB88BB96E6D7F45F2C0EEF4906D87B761413626C925F3DAAA483B058E70F
	Remove-Item C:\TEMP\buffer.csv

	Remove-Item function:\Test-6

	# REPL $r
	Assert-Far ($r | Join-String -Separator /) -eq 'Line1/Line2'
}
