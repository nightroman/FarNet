<#
.Synopsis
	Mock tests of Open-TextLink.ps1
#>

# mock

$r = @{}
Set-Alias Open-Match Open-Match2
function Open-Match2 {
	$r.File = $Matches['File']
	$r.Text = $Matches['Text']
	$r.Line = $Matches['Line']
	$r.Char = $Matches['Char']
}

task PS.script-stack {
	Open-TextLink.ps1 'at *DP, C:\ROM\KIT\IB\Invoke-Build.ps1: line 142'
	equals $r.File C:\ROM\KIT\IB\Invoke-Build.ps1
	equals $r.Line '142'
}

task ClearScript.with-function-name {
	Open-TextLink.ps1 "    at test (C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js:38:11) ->     throw Error('OK')"
	equals $r.File C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js
	equals $r.Text "throw Error('OK')"
	equals $r.Line '38'
	equals $r.Char '11'
}

task ClearScript.from-call-stack {
	Open-TextLink.ps1 "    at C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js:41:1"
	equals $r.File C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js
	equals $r.Text $null
	equals $r.Line '41'
	equals $r.Char '1'
}

# contrived
task ClearScript.simple-with-env {
	Open-TextLink.ps1 "  at  %FarNetCode%\Test\Utility\Test-TextLink.fas.ps1:8:10  ->  *** TEST DATA"
	equals $r.File '%FarNetCode%\Test\Utility\Test-TextLink.fas.ps1'
	equals $r.Text '*** TEST DATA'
	equals $r.Line '8'
	equals $r.Char '10'
}
