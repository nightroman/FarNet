<#
.Synopsis
	Console output with UI.

.Description
	If it fails with md5 test, watch the output: if it is OK then update the
	control md5. The file c:\temp\buffer.csv contains the screen buffer data.
	Mind the colors, e.g. command echo.

	Empty ends of lines filled by spaces may have unexpected ForegroundColor
	presumably defined by some previous lines with solid ends of lines.

	* Used to fails in step-by-step mode due to unexpected screen UI.
	* 2016-07-02 http://forum.farmanager.com/viewtopic.php?p=139142#p139142
#>

job {
	$Data.MacroOutput = [FarNet.Works.Kit]::MacroOutput
	[FarNet.Works.Kit]::MacroOutput = $true

	# Input()
	function global:test1
	{
		'before'
		$Far.Input('test')
		'after'
	}

	# Message()
	function global:test2
	{
		'before'
		$Far.Message('test')
		'after'
	}
}

job {
	# panels off
	#1
	$Far.Panel2.IsVisible = $false
	#2
	$Far.Panel.IsVisible = $false
}

# 5.0.94 error color should be red
job {
	$Data.errorCount = $global:Error.Count
}
macro 'Keys"c l s Enter P S : Space t h r o w Space 4 2 Enter"'
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = New-Object System.Management.Automation.Host.Rectangle 0, ($y - 9), 30, ($y - 6)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv -NoTypeInformation c:\temp\buffer.csv
	$md5 = [guid][System.Security.Cryptography.MD5]::Create().ComputeHash([System.IO.File]::ReadAllBytes('C:\TEMP\buffer.csv'))
	Assert-Far $md5 -eq ([guid]'2892b82f-bde7-3ff6-c90d-eaf91ad5038f')
	Remove-Item C:\TEMP\buffer.csv
	Assert-Far $Data.errorCount -eq ($global:Error.Count - 1)
	$global:Error.RemoveAt(0)
}

### used to be user screen mess
<#
ps: test1 --- DarkDray
before


after

#>
macro 'Keys"c l s Enter P S : Space t e s t 1 Enter Enter"'
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = New-Object System.Management.Automation.Host.Rectangle 0, ($y - 4), 8, ($y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv -NoTypeInformation c:\temp\buffer.csv
	$md5 = [guid][System.Security.Cryptography.MD5]::Create().ComputeHash([System.IO.File]::ReadAllBytes('C:\TEMP\buffer.csv'))
	Assert-Far $md5 -eq ([guid]'e1927daf-06fe-34a5-0164-59c828d21b78')
	Remove-Item C:\TEMP\buffer.csv
}

### 'before' was missing
<#
ps: test1 --- DarkDray
before

after

#>
macro 'Keys"c l s Enter P S : Space t e s t 2 Enter Enter"'
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = New-Object System.Management.Automation.Host.Rectangle 0, ($y - 3), 8, ($y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv -NoTypeInformation c:\temp\buffer.csv
	$md5 = [guid][System.Security.Cryptography.MD5]::Create().ComputeHash([System.IO.File]::ReadAllBytes('C:\TEMP\buffer.csv'))
	Assert-Far $md5 -eq ([guid]'b106cf3f-7956-4d98-414d-ebe3efa0aaef')
	Remove-Item C:\TEMP\buffer.csv
}

### full output
macro @"
 Keys("c l s Enter p s : Space")
 print("$("$env:PSF\Samples\Tests\Test-Write.ps1".Replace('\', '\\'))")
 Keys("Enter")
"@
job {
	Assert-Far @(
		$global:Error.Count -ge 2
		$global:Error[0].ToString() -eq "Test of Write-Error 2"
		$global:Error[1].ToString() -eq "Test of Write-Error 1"
	)
	$global:Error.RemoveAt(0)
	$global:Error.RemoveAt(0)
}
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = New-Object System.Management.Automation.Host.Rectangle 0, ($y - 22), 32, ($y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv -NoTypeInformation c:\temp\buffer.csv
	#! used to get different MD5 in different PS versions
	$result = [guid][System.Security.Cryptography.MD5]::Create().ComputeHash([System.IO.File]::ReadAllBytes('C:\TEMP\buffer.csv'))
	$sample = [guid]'170f2c3d-1d3f-19f5-5010-26ed8fe8c89e'
	Assert-Far $sample -eq $result
	Remove-Item C:\TEMP\buffer.csv
}

### viewer output simple
macro 'Keys"v p s : 1 2 3 4 5 Enter"'
job {
	Assert-Far -Viewer
	Assert-Far $Far.Viewer.Title -eq '12345'
}
keys Esc
job {
	Assert-Far -Panels
}

### viewer output + switch
macro 'Keys"v p s : 1 2 3 4 5 Enter"'
job {
	Assert-Far -Viewer
	Assert-Far $Far.Viewer.Title -eq '12345'
}
keys F6
job {
	Assert-Far -EditorFileName *PowerShell_transcript.*.*.*.txt
}
keys Esc
job {
	# I am not happy with this dialog but it is
	Assert-Far -Dialog
}
keys n
job {
	Assert-Far -Panels
}

### end
job {
	$Far.Panel.IsVisible = $true
}
job {
	$Far.Panel2.IsVisible = $true
}
job {
	Clear-Host
	Remove-Item Function:\test1,  Function:\test2
	[FarNet.Works.Kit]::MacroOutput = $Data.MacroOutput
}
