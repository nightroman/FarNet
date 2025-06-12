<#
.Synopsis
	Tests TabExpansion2 and ArgumentCompleters.

.Description
	Should be tested with FarHost, pwsh, powershell.
#>

$ErrorActionPreference = 1
$Error.Clear()

$FarHost = $Host.Name -eq 'FarHost'
$v740 = $PSVersionTable.PSVersion -ge ([version]'7.4.0')

Set-Alias assert Assert-Test
Set-Alias ib Invoke-Build

# Ensure TabExpansion2 is loaded
& "$env:FarNetCode\PowerShellFar\TabExpansion2.ps1"

# Set location, we assume some files
Set-Location -LiteralPath $PSScriptRoot

# Invokes TabExpansion and tests the results.
function Test([Parameter()]$line, $assert, $caret=$line.Length) {
	$private:line_ = $line
	$private:assert_ = $assert
	$private:caret_ = $caret
	Remove-Variable line, assert, caret

	try {
		$_ = foreach($_ in (TabExpansion2 $line_ $caret_).CompletionMatches) { $_.CompletionText }
	}
	catch {
		Write-Error ($_ | Out-String)
	}
	if (!(. $assert_)) {
		Write-Host -ForegroundColor Yellow @"
Line   : $line_
Result : $($_ -join '|')
"@
		Write-Error 'TabExpansion test failed.'
	}
}

function Assert-Test([Parameter()][scriptblock]$Condition) {
	$_ = & $Condition
	if ($_ -isnot [bool]) {Write-Error "Assertion not Boolean: {$Condition}"}
	if (!$_) {Write-Error "Assertion failed: {$Condition}"}
}

function Assert-NoError {
	if ($PSEdition -eq 'Core') {
		if ($Error) {Write-Error "Unexpected error:`n$($Error[-1])"}
	}
	else {
		$Error | .{process{
			if ($_.Message -notlike '*System.Security.AccessControl.ObjectSecurity*') {Write-Error "Unexpected error:`n$_"}
		}}
		$Error.Clear()
	}
}

# GO
$sw = [System.Diagnostics.Stopwatch]::StartNew()

### static member: property, method and with prefix

Test '[datetime]::no' { $_ -ceq 'Now' }
Test '[datetime]::fromb' { $_ -ceq 'FromBinary(' }
Test 'Msg(([IO.File]::ref' { $_ -ceq 'ReferenceEquals(' }

### variables and members of a variable

Test '$hos' { $_ -ceq '$Host' }
Test '$global:hos' { $_ -ceq '$global:Host' }
Test '$script:hos' { $_ -ceq '$script:Host' }
Test '$local:hos' { $_ -ceq '$local:Host' }
Test '$host.u' { $_ -ceq 'UI' }
Test '$host.ui.r' { $_[0] -ceq 'RawUI' }
Test '$Host.UI.RawUI.e' { $_ -ceq 'Equals(' }
Test '@psb' { $_ -ccontains '@PSBoundParameters' }

# base member
$xml = [xml]'<tests><test>test</test></tests>'
Test '$xml.ou' { $_ -ceq 'OuterXml'}

# adapted member
Test '$xml.tes' { $_ -ceq 'tests'}

### variable wildcard pattern

Test @'
ls
$*my
'@ { $_ -ccontains '$MyInvocation' }

### members of a static object

Test '[system.datetime]::Now.h' { $_ -ceq 'Hour' }

### members of a simple expression

Test '(get-date).h' { $_ -ceq 'Hour' }
Test '(1 + 1).to' { $_ -ceq 'ToString(' }
Test "('string').le" { $_ -ceq 'Length' }

### cmdlet parameters

Test 'gc -p' { $_ -ceq '-Path' }
Test 'Get-Content -p' { $_ -ceq '-Path' }
Test 'ls -' { $_ -ccontains '-Path' -and $_ -ccontains '-LiteralPath' }
Test '$e = ls -' { $_ -ccontains '-Path' -and $_ -ccontains '-LiteralPath' }
# _091023_204251 E.g. definition of a hashtable
Test '@{Name = Split-Path -l' { $_ -ccontains '-Leaf' -and $_ -ccontains '-LiteralPath' }

### scripts, their aliases and parameters

Test @'
ls
assert
'@ { $_ -ceq 'Assert-Test' }
Test 'Panel-Bits' { $_ -ceq 'Panel-BitsTransfer.ps1' }
Test 'Panel-DBData -col' { $_ -ceq '-Columns' }

# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' { $_ -ccontains '-prm1' }

### function, filter, script-cmdlet parameters

# function or filter
Test 'TabExpansion2 -' { $_[0] -ceq '-inputScript' }
# unnamed cmdlet
Test 'rv -' { $_ -ccontains '-Name' }
# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' { $_ -ccontains '-prm1' }

### tricky parameters

Test '@(Get-Content $file -TotalCount ($x + $y) -e' { $_ -ccontains '-ErrorAction' }

### drive

Test 'alias:epc' { $_ -ceq 'Alias:\epcsv' }
Test '$alias:epc' { $_ -ceq '$alias:epcsv' }
Test 'env:computern' { $_ -ceq 'Env:\COMPUTERNAME' }
Test '$env:computern' { $_ -ceq '$env:COMPUTERNAME' }
Test 'function:tabexp' { $_ -ceq 'Function:\TabExpansion2' }
Test '$function:tabexp' { $_ -ceq '$function:TabExpansion2' }

### scope

Test '$global:$host.ui.ra' { $_ -ceq 'RawUI' }

### alias

# contains definition as the first item and does not contain itself
Test @'
ls
rm
'@ { $_[0] -ceq 'Remove-Item' -and $_ -notcontains 'rm' }

#! 2+ aliases are not completed but built-in should work
Test @'
ls
rm*
'@ { $_ -ccontains 'rm' }
Assert-NoError

### full path

Test 'c:\prog*\common' { $_ -ceq "& 'C:\Program Files\Common Files'" }
Test 'HKCU:\Cons' { $_ -ceq 'HKCU:\Console' }

### with prefixes

Test ';({|$hos' { $_ -ceq '$Host' }
Test '"test-' { $_ -ccontains '".\Test-TabExpansion2.far.ps1"' }
Test "'test-" { $_ -ccontains "'.\Test-TabExpansion2.far.ps1'" }
Test '"$(test-' { $_ -ccontains '.\Test-TabExpansion2.far.ps1' }

### function

Test 'Clear-H' { $_ -ccontains 'Clear-Host' }

### type wildcards

# wildcards
Test '[.nullr' { $_ -ccontains '[System.NullReferenceException' }
Test '[*commandty' { $_ -ccontains '[System.Data.CommandType' }
Test '[*sqlcom' { $_ -ccontains '[System.Data.SqlTypes.SqlCompareOptions' }

# Generics
Test '[System.Collections.Generic.h*' { $_ -ccontains '[System.Collections.Generic.HashSet' }
Test '[*Collections.Generic.d' { $_ -ccontains '[System.Collections.Generic.Dictionary' }

### help comments

Test "<#`r`n." { $_ -ccontains '.Synopsis' -and $_ -ccontains '.Link' }
Test "<#`r`n.s" { $_ -ceq '.Synopsis' }
Test "<#`r`n .s" { $_ -ceq '.Synopsis' }
Test "<#`r`n`t.s" { $_ -ceq '.Synopsis' }

Test '#.e' { $_ -ccontains '.ExternalHelp' }
Test '#  .e' { $_ -ccontains '.ExternalHelp' }
Test '  ##  .e' { $_ -ccontains '.ExternalHelp' }

Test "<#`r`n.z" { !$_ }
Test "#.z" { !$_ }

### code in comments

Test '<# ls' { $_ -ccontains 'Get-ChildItem' }

Test @'
ls
# Get-Content -e
'@ { $_ -ccontains '-ErrorAction' }

Test @'
ls
gc ## ls
'@ { $_ -ccontains 'Get-ChildItem' }

Test @'
<#
ls
'@ { $_ -ccontains 'Get-ChildItem' }

Test @'
<#
  ## ls
'@ { $_ -ccontains 'Get-ChildItem' }

# fixed loop
Test '#' { $true }

### fix of $Line.[Tab] (name is exactly 'LINE')
if ($FarHost) {
	$Line = $Far.CommandLine
	Test '$Line.' { $_ -ccontains 'ActiveText' }
}

### Find-FarFile
if ($FarHost) {
	Test 'Find-FarFile ' { "$_" -ceq "$($Far.Panel.GetFiles())" }
	Test 'Find-FarFile zzz' { !$_ -and !$Error }
}

### Out-FarPanel
if ($FarHost) {
	Test 'Get-ChildItem | Out-FarPanel ' { $_[0] -ceq 'Attributes' -and $_[-1] -ceq "@{e=''; n=''; k=''; w=0; a=''}" }
	Test 'Out-FarPanel ' { $_ -ceq "@{e=''; n=''; k=''; w=0; a=''}" }
}

### ComputerName

Test 'Invoke-Command -ComputerName ' { @($_)[0] -ceq $env:COMPUTERNAME }

### git

Test 'git ' { $_ -ccontains 'clean' }
Test 'git a' { $_ -ccontains 'add' }

### Mdbc
if ($PSEdition -eq 'Core') {
	. $PSScriptRoot\Test-TabExpansion2-Mongo.ps1
}

### Invoke-Build, alias x

Push-Location $env:FarNetCode\Zoo
Test "ib " { $_ -ccontains 'setVersion' -and $_ -ccontains 'zipFarDev' }
Test "ib -File ReleaseFarNet.build.ps1 p" { $_ -ccontains 'pushSource' -and $_ -cnotcontains 'commitSource' }
Test "ib . " { $_ -eq 'PowerShellFar' -and $_ -like '*.ps1'  }
Test "ib ** " { $_.Count -ge 3 -and $_ -eq 'PowerShellFar' -and @($_ -like '*.ps1').Count -eq 0 }
Pop-Location

### whole script

#! use of $env: gives a silent error
Test (@'
ib  `
-File {0}
'@ -f "$env:FarNetCode\Zoo\ReleaseFarNet.build.ps1") -caret 3 { $_ -ccontains 'zipFarDev' }

### Equals, GetType, ToString

Test '$Missing1.e' {'Equals(' -ceq $_}
Test '( $Missing1 ).g' {'GetType()' -ceq $_}
Test '($Missing1 + $Missing2).t' {'ToString()' -ceq $_}

### weird cases

# complete $*var, ensure no leaked variables
Test '$*' {
	($_ -notcontains '$inputScript') -and
	($_ -notcontains '$cursorColumn') -and
	($_ -notcontains '$ast') -and
	($_ -notcontains '$tokens') -and
	($_ -notcontains '$positionOfCursor') -and
	($_ -notcontains '$options')
}

# 1.0.4 - try insert space
Test '$hos_' -caret 4 {$_ -contains '$Host'}
Test '$hos1' -caret 4 {$_ -contains '$Host'}
Test '$hosZ' -caret 4 {$_ -contains '$Host'}
Test '$hos$' -caret 4 {$_ -contains '$Host'}
# 1.0.6 - ditto on any \S
Test '$hos"' -caret 4 {$_ -contains '$Host'}

# OK
Assert-NoError
"Done TabExpansion test, $($sw.Elapsed)"
