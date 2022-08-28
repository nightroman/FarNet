<#
.Synopsis
	Mock tests of Open-TextLink.ps1
	See UI tests -- Utility\Test-TextLink.fas.ps1
#>

# mock

$r = @{}
Set-Alias Open-Match Open-Match2
function Open-Match2 {
	$r.File = $Matches.File
	$r.Text = $Matches.Text
	$r.Line = $Matches.Line
	$r.Char = $Matches.Char
}

### ClearScript links

# with function name
Open-TextLink.ps1 "    at test (C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js:38:11) ->     throw Error('OK')"
Assert-Far $r.File -eq C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js
Assert-Far $r.Text -eq "throw Error('OK')"
Assert-Far $r.Line -eq '38'
Assert-Far $r.Char -eq '11'

# from call stack
Open-TextLink.ps1 "    at C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js:41:1"
Assert-Far $r.File -eq C:\ROM\FarDev\Code\JavaScriptFar\Samples\task-with-error.js
Assert-Far $r.Text -eq $null
Assert-Far $r.Line -eq '41'
Assert-Far $r.Char -eq '1'

# simple with env (contrived)
Open-TextLink.ps1 "  at  %FarNetCode%\Test\Utility\Test-TextLink.fas.ps1:8:10  ->  *** TEST DATA"
Assert-Far $r.File -eq '%FarNetCode%\Test\Utility\Test-TextLink.fas.ps1'
Assert-Far $r.Text -eq '*** TEST DATA'
Assert-Far $r.Line -eq '8'
Assert-Far $r.Char -eq '10'
