<#
.Synopsis
	Tests TabExpansion2.
#>

$ErrorActionPreference = 1
$Error.Clear()

# Ensure TabExpansion2 is loaded in the FarHost
if ($Host.Name -ceq 'FarHost') {
	& "$($Psf.AppHome)\TabExpansion2.ps1"
}

# Set location in here, we assume and use some files
Set-Location -LiteralPath (Split-Path $MyInvocation.MyCommand.Path)

# Invokes TabExpansion and tests the results.
function Test([Parameter()]$line, $assert, $caret=$line.Length)
{
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
		Write-Error @"
TabExpansion test failed
Line   : $line_
Result : $($_ -join '|')
"@
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
pp
'@ { $_ -ceq 'Get-FarPath' }
Test 'Panel-Bits' { $_ -ceq 'Panel-BitsTransfer.ps1' }
Test 'Panel-DbData- -col' { $_ -ceq '-Columns' }

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
Assert-Far (!$Error)

### full path

Test 'c:\prog*\common' { $_ -ceq "& 'C:\Program Files\Common Files'" }
Test 'HKCU:\Cons' { $_ -ceq 'HKCU:\Console' }

### with prefixes

Test ';({|$hos' { $_ -ceq '$Host' }
Test '"test-' { $_ -ccontains '".\Test-TabExpansion2-.ps1"' }
Test "'test-" { $_ -ccontains "'.\Test-TabExpansion2-.ps1'" }
Test '"$(test-' { $_ -ccontains '.\Test-TabExpansion2-.ps1' }

### function

Test 'Clear-H' { $_ -ccontains 'Clear-Host' }

### types and namespaces

# explicit namespace and type names
Test @'
ls
[sy
'@ { [string]$_ -ceq '[System. [SystemException]' }
Test '[mi' { $_ -ccontains '[Microsoft.' }
Test '[system.da' { $_ -ccontains '[System.Data.' }
Test '[system.data.sq' { $_ -ccontains '[System.Data.SqlClient.' }
Test '[system.data.sqlclient.sql' { $_ -ccontains '[System.Data.SqlClient.SqlClientPermission]' }

# order: namespaces then classes
Test '[System.Management.aut' { ($_ -join '=') -match '\[System\.Management\.Automation\.=.*\[System\.Management\.AuthenticationLevel\]' }

# wildcard namespace and type names
Test '[*commandty*' { $_ -ccontains '[System.Data.CommandType]' }
Test '[*sqlcom*' { $_ -ccontains '[System.Data.SqlTypes.SqlCompareOptions]' }

# Generics
Test '[Collections.Generic.h' { $_ -ccontains '[Collections.Generic.HashSet[]]' }
Test '[Collections.Generic.d' { $_ -ccontains '[Collections.Generic.Dictionary[,]]' }

# 2013-12-16
Test '$algo.ComputeHash(([IO' { $_ -ccontains '$algo.ComputeHash(([IO.' }

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

### fix of $Line.[Tab] (name is exactly 'LINE')

$Line = $Far.CommandLine
Test '$Line.' { $_ -ccontains 'ActiveText' }

### Find-FarFile

Test 'Find-FarFile ' { "$_" -ceq "$($Far.Panel.GetFiles())" }

Test 'Find-FarFile zzz' { !$_ -and !$Error }

### Out-FarPanel

Test 'Get-ChildItem | Out-FarPanel ' { $_[0] -ceq 'Attributes' -and $_[-1] -ceq "@{e=''; n=''; k=''; w=0; a=''}" }
Test 'Out-FarPanel ' { $_ -ceq "@{e=''; n=''; k=''; w=0; a=''}" }

### ComputerName

Test 'Invoke-Command -ComputerName ' { @($_)[0] -ceq $env:COMPUTERNAME }

### git

Test 'git ' { $_ -ccontains 'clean' }
Test 'git a' { $_ -ccontains 'add' }

### =-patterns

Test @'
ls
$=
'@ { ($_ -ccontains '$LastExitCode') -and ($_ -notcontains '$Error') }

Test @'
ls
val=
'@ { $_ -ccontains 'ValueFromPipeline=$true' -and $_ -ccontains '[ValidateCount(#, )]' }

#1
Assert-Far (!$Error)
Test '[val=' { $_ -ccontains '[ValidateCount(#, )]' }
# v3.0 wildcard pattern is not valid
# v4.0 wildcard character pattern is not valid
Assert-Far ($Error.Count -eq 1 -and $Error -match 'pattern is not valid: \\\[val=\*')
$Error.Clear()

#2 fixed: it's the AstInputSet parameter set, work around read only should work, too.
#! There is no error, ulike above.
Test @'
ls
[val=
'@ { $_ -ccontains '[ValidateCount(#, )]' }
Assert-Far (!$Error)

### Mdbc
. $PSScriptRoot\Test-TabExpansion2-Mongo.ps1

### Invoke-Build, alias x

# assume two build files
Push-Location $env:FarNetCode\Zoo

# Task
Test "ib " { $_ -ccontains 'release' -and $_ -ccontains 'testNuGet' -and $_ -cnotcontains 'pushSource' }
Test "ib R" { $_ -ccontains 'release' -and $_ -cnotcontains 'testNuGet' }
Test "ib -File ReleaseFarNet.build.ps1 " { $_ -ccontains 'pushSource' -and $_ -ccontains 'commitSource' -and $_ -cnotcontains 'testZip' }
#_190903_023748 (see ReleaseFarNet.build.ps1)
Test "ib -File ReleaseFarNet.build.ps1 p" { $_ -ccontains 'pushSource' -and $_ -cnotcontains 'commitSource' }

# File - directories .ps1 files
Test "ib . " { $_ -eq 'PowerShellFar' -and $_ -like '*.ps1'  }

# ** File - directories only
Test "ib ** " { $_.Count -ge 3 -and $_ -eq 'PowerShellFar' -and @($_ -like '*.ps1').Count -eq 0 }

Pop-Location

### whole script

#! use of $env: gives a silent error
Test (@'
ib  `
-File {0}
'@ -f "$env:FarNetCode\Zoo\.build.ps1") -caret 3 { $_ -ccontains 'zipFarDev' }

### Equals, GetType, ToString

Test '$Missing1.e' {'$Missing1.Equals(' -ceq $_}
Test '( $Missing1 ).g' {').GetType()' -ceq $_}
Test '($Missing1 + $Missing2).t' {'$Missing2).ToString()' -ceq $_}

### weird cases

# work around read only result
Test '    [Al=' { $_ -ccontains "[Alias('#')]" }

# complete $*var, ensure no leaked variables
Test '$*' {
	($_ -notcontains '$inputScript') -and
	($_ -notcontains '$cursorColumn') -and
	($_ -notcontains '$ast') -and
	($_ -notcontains '$tokens') -and
	($_ -notcontains '$positionOfCursor') -and
	($_ -notcontains '$options')
}

# 1.0.2 fixed regression
#! 1.0.2 still worked fine for a single line test, so use 2 lines
$text = "`r`n[cmd="
$r1 = TabExpansion2 $text $text.Length
$r2 = $r1.CompletionMatches
Assert-Far @(
	$r2.Count.Equals(2) #! may change
	$r2[0].CompletionText.Equals('[CmdletBinding(#)]')
	$r1.ReplacementIndex.Equals(2)
	$r1.ReplacementLength.Equals(5)
)

# 1.0.4 - try insert space
Test '$hos_' -caret 4 {$_ -contains '$Host'}
Test '$hos1' -caret 4 {$_ -contains '$Host'}
Test '$hosZ' -caret 4 {$_ -contains '$Host'}
Test '$hos$' -caret 4 {$_ -contains '$Host'}

# OK
Assert-Far $Error.Count -eq 0
"Done TabExpansion test, $($sw.Elapsed)"
