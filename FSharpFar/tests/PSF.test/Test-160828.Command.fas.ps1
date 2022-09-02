
macro "print 'fs: let x = 42'; Keys'Enter' -- invoke code"
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = New-Object System.Management.Automation.Host.Rectangle 0, ($y - 3), 20, ($y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv -NoTypeInformation c:\temp\buffer.csv
	$md5 = (Get-FileHash C:\TEMP\buffer.csv -Algorithm MD5).Hash
	Assert-Far $md5 -eq '92ECA6CDC8B9C6BD33BF4B22A8BA881A'
	Remove-Item C:\TEMP\buffer.csv
}
