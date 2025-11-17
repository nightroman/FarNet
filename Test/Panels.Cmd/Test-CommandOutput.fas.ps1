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
	$__.IsVisible = $false
}

# 5.0.94 error color should be red
job {
	$Data.errorCount = $global:Error.Count
}
keys P S : Space t h r o w Space 4 2 Enter
job {
	# from ps: to end
	$y = $Host.UI.RawUI.CursorPosition.Y
	# empty line and red message
	$rect = [System.Management.Automation.Host.Rectangle]::new(0, $y - 2, 30, $y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv C:\TEMP\buffer.csv
	$hash = (Get-FileHash C:\TEMP\buffer.csv).Hash
	Assert-Far $hash -eq A84385C2E00B7978B0A656295F2E98B124C4D7695D3E7BB773F1D27F92378FC0
	Remove-Item C:\TEMP\buffer.csv
	Assert-Far $Data.errorCount -eq ($global:Error.Count - 1)
	$global:Error.RemoveAt(0)
}

### used to be user screen mess
<#

before


after
#>
keys P S : Space t e s t 1 Enter Enter
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = [System.Management.Automation.Host.Rectangle]::new(0, $y - 4, 8, $y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv C:\TEMP\buffer.csv
	$hash = (Get-FileHash C:\TEMP\buffer.csv).Hash
	Assert-Far $hash -eq A134E94D8BF0461FA3B153CE525E00D6DCE91B69F4AFC77C12373C6B2305EC53
	Remove-Item C:\TEMP\buffer.csv
}

### 'before' was missing
<#

before
after
#>
keys P S : Space t e s t 2 Enter Enter
job {
	$y = $Host.UI.RawUI.CursorPosition.Y
	$rect = [System.Management.Automation.Host.Rectangle]::new(0, $y - 3, 8, $y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv C:\TEMP\buffer.csv
	$hash = (Get-FileHash C:\TEMP\buffer.csv).Hash
	Assert-Far $hash -eq 68E89467FF72CE3F47FAAA594C6DED43D37F0BBE64B633E138E5B2D68C250B7F
	Remove-Item C:\TEMP\buffer.csv
}

### full output
<#

Test of Write-Host
...
Write-Error: Test of Write-Error 2
#>
macro @"
 Keys "p s : Space"
 print("$("$env:FarNetCode\Samples\Tests\Test-Write.ps1".Replace('\', '\\'))")
 Keys "Enter"
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
	# from line starting `ps:` to line ending `Error 2`
	$rect = [System.Management.Automation.Host.Rectangle]::new(0, $y - 11, 34, $y - 1)
	$buff = $Host.UI.RawUI.GetBufferContents($rect)
	$buff | Select-Object Character, ForegroundColor | Export-Csv C:\TEMP\buffer.csv
	$hash = (Get-FileHash C:\TEMP\buffer.csv).Hash
	Assert-Far $hash -eq 16AF8F69E031AFDDFB0810F07FF6A7131E29E65B1D3908A32FCC4479A742FDF6
	Remove-Item C:\TEMP\buffer.csv
}

### viewer output simple
keys v p s : 1 2 3 4 5 Enter
job {
	Assert-Far -Viewer
	Assert-Far $__.Title -eq '12345'
}
keys Esc
job {
	Assert-Far -Panels
}

### viewer output + switch
keys v p s : 1 2 3 4 5 Enter
job {
	Assert-Far -Viewer
	Assert-Far $__.Title -eq '12345'
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
	$__.IsVisible = $true
}
job {
	$Far.Panel2.IsVisible = $true
}
job {
	Remove-Item Function:\test1,  Function:\test2
	[FarNet.Works.Kit]::MacroOutput = $Data.MacroOutput
	[FarNet.Tasks]::WaitForPanels(999)
}
