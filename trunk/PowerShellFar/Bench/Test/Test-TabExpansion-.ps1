
<#
.Synopsis
	Test TabExpansion.
	Author: Roman Kuzmin

.Description
	This test is for the PowerShellFar development environment. It may fail in
	a different environment. But with adjustments most of the test should work.

.Notes
	(Get-Content -e --> -Encoding) may fail depending on the current provider,
	because it is a dynamic parameter. We test TabExpansion in here, not
	PowerShell provider cmdlets issues, so do not use Encoding.

	Fixed [_080510_023705]: V2 CTP2 or CTP1: 'param' is parsed as 'command' in
	script cmdlets. 090112: V2 CTP3 - fixed.
#>

# Ensure TabExpansion is loaded
if ($Host.Name -ceq 'FarHost') {
	. "$($Psf.AppHome)\TabExpansion.ps1"
}

# Set location in here, we assume and use some files
Set-Location -LiteralPath (Split-Path $MyInvocation.MyCommand.Path)

# Invokes TabExpansion and tests the results.
function Test([Parameter()]$line_, $word_, $assert_)
{
	$_ = TabExpansion $line_ $word_
	if (!(. $assert_)) {
		Write-Error -ErrorAction 1 @"
*** TabExpansion test failed
line = $line_
word = $word_
result:
$($_ -join "`n")
"@
	}
}

# GO
$time1 = [DateTime]::Now

### static member: property, method and with prefix
Test '' '[datetime]::no' { $_ -ceq '[datetime]::Now' }
Test '' '[datetime]::fromb' { $_ -ceq '[datetime]::FromBinary(' }
Test '' 'Msg(([IO.File]::ref' { $_ -ceq 'Msg(([IO.File]::ReferenceEquals(' }

### variables and members of a variable
Test '' '$hos' { $_ -ceq '$Host' }
Test '' '@psb' { $_ -ccontains '@PSBoundParameters' }
Test '' '$global:hos' { $_ -ceq '$global:Host' }
Test '' '$script:hos' { $_ -ceq '$script:Host' }
Test '' '$local:hos' { $_ -ceq '$local:Host' }
Test '' '$host.u' { $_ -ceq '$host.UI' }
Test '' '$host.ui.r' { $_[0] -ceq '$host.ui.RawUI' }
Test '' '$Host.UI.RawUI.e' { $_ -ceq '$Host.UI.RawUI.Equals(' }
# base member
$xml = [xml]'<tests><test>test</test></tests>'
Test '' '$xml.ou' { $_ -ceq '$xml.OuterXml'}
# adapted member
Test '' '$xml.tes' { $_ -ceq '$xml.tests'}
# wildcard
Test '' '$*var' { $_ -ccontains '$MaximumVariableCount' }

### members of a static object
Test '' '[system.datetime]::Now.h' { $_ -ceq '[system.datetime]::Now.Hour' }

### members of a simple expression
Test '(get-date).h' '(get-date).h' { $_ -ceq '(get-date).Hour' }
Test '(1 + 1).to' '1).to' { $_ -ceq '1).ToString(' }
Test "('string').le" "('string').le" { $_ -ceq "('string').Length" }

### cmdlet parameters
Test 'gc -p' '-p' { $_ -ceq '-Path' }
Test 'Get-Content -p' '-p' { $_ -ceq '-Path' }
Test 'ls -' '-' { $_ -ccontains '-Path' -and $_ -ccontains '-LiteralPath' }
Test '$e = ls -' '-' { $_ -ccontains '-Path' -and $_ -ccontains '-LiteralPath' }
# _091023_204251 E.g. definition of a hashtable
Test 'Name = Split-Path -l' '-l' { $_ -ccontains '-Leaf' -and $_ -ccontains '-LiteralPath' }

### scripts, their aliases and parameters
Test '' 'pd' { $_ -ceq 'Panel-DbData-' }
Test '' 'Panel-Bits' { $_ -ceq 'Panel-BitsTransfer-.ps1' }
Test 'pd -col' '-col' { $_ -ceq '-Columns' }
Test 'Panel-DbData- -col' '-col' { $_ -ceq '-Columns' }

# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' '-' { $_ -ccontains '-prm1' }

### function, filter, script-cmdlet parameters
# function or filter
Test 'GetScriptParameter -' '-' { $_[0] -ceq '-Path' }
# unnamed cmdlet
Test 'man -' '-' { $_ -ccontains '-Name' }
# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' '-' { $_ -ccontains '-prm1' }

### tricky parameters
Test '# Get-Content -e' '-e' { $_ -ccontains '-ErrorAction' }
Test '>: Get-Content -e' '-e' { $_ -ccontains '-ErrorAction' }
Test '@(Get-Content $file -TotalCount ($x + $y) -e' '-e' { $_ -ccontains '-ErrorAction' }
Test '"@(Get-Content $file -TotalCount ($x + $y) -e' '-e' { $_ -ccontains '-ErrorAction' }
Test "'@(Get-Content `$file -TotalCount (`$x + `$y) -e" '-e' { $_ -ccontains '-ErrorAction' }

### drive
Test '' 'alias:epc' { $_ -ceq 'alias:epcsv' }
Test '' '$alias:epc' { $_ -ceq '$alias:epcsv' }
Test '' 'env:computern' { $_ -ceq 'env:COMPUTERNAME' }
Test '' '$env:computern' { $_ -ceq '$env:COMPUTERNAME' }
Test '' 'function:tabexp' { $_ -ceq 'function:TabExpansion' }
Test '' '$function:tabexp' { $_ -ceq '$function:TabExpansion' }

### scope
Test '' '$global:$host.ui.ra' { $_ -ceq '$global:$host.ui.RawUI' }

### alias
Test '' 'gc' { $_ -ceq 'Get-Content' }

### full path
Test '' 'c:\prog*\common' { $_ -ceq 'C:\Program Files\Common Files' }
Test '' 'HKCU:\Cons' { $_ -ceq 'HKCU:\Console' }

### with prefixes
Test '' ';({|$hos' { $_ -ceq ';({|$Host' }
Test '' '!test-' { $_ -ccontains '!Test-Base-.ps1' }
Test '' '"test-' { $_ -ccontains '"Test-Base-.ps1' }
Test '' "'test-" { $_ -ccontains "'Test-Base-.ps1" }
Test '' '"$(test-' { $_ -ccontains '"$(Test-Base-.ps1' }

### function
Test '' 'Clear-H' { $_ -ccontains 'Clear-Host' }

### namespace and type names

# add used types before the tests
Add-Type -AssemblyName System.Windows.Forms

# explicit namespace and type names
Test '' '[sy' { [string]$_ -ceq '[System. [SystemException]' }
Test '' '[mi' { $_ -ccontains '[Microsoft.' }
Test '' '[system.da' { $_ -ccontains '[System.Data.' }
Test '' '[system.data.sq' { $_ -ccontains '[System.Data.Sql.' }
Test '' '[system.data.sqlclient.sqle' { $_[0] -ceq '[System.Data.SqlClient.SqlError]' }

# wildcard namespace and type names
Test '' '[*commandty*' { $_ -ccontains '[System.Data.CommandType]' }
Test '' '[*sqlcom*' { $_ -ccontains '[System.Data.SqlClient.SqlCommand]' }

# Generics
Test '' '[Collections.Generic.h' { $_ -ccontains '[Collections.Generic.HashSet`1]' }

# New-Object namespace and type names
Test 'NEW-OBJECT System.da' 'System.da' { $_ -ccontains 'System.Data.' }
Test 'NEW-OBJECT  -TYPENAME  System.da' 'System.da' { $_ -ccontains 'System.Data.' }
Test 'NEW-OBJECT   System.Data.SqlClient.SqlE' 'System.Data.SqlClient.SqlE' { $_[0] -ceq 'System.Data.SqlClient.SqlError' }
Test 'New-Object System.Data.SqlClient.' 'System.Data.SqlClient.' { $_.Count -gt 1 }
# 100728 Weird: $set1.Keys is kind of $null for System.Windows.Forms.[Tab], so we use there GetEnumerator()
# 131123 NA, cache is not used
Test 'New-Object System.Windows.Forms.' 'System.Windows.Forms.' { $_.Count -gt 1 }

### Module name
Test 'IMPORT-MODULE b' 'b' { $_ -ccontains 'BitsTransfer' }
Test 'IPMO b' 'b' { $_ -ccontains 'BitsTransfer' }
Test 'IMPORT-MODULE -NAME b' 'b' { $_ -ccontains 'BitsTransfer' }
Test 'IPMO -NAME b' 'b' { $_ -ccontains 'BitsTransfer' }

### Process name
Test 'GET-PROCESS f' 'f' { $_ -ccontains 'Far' }
Test 'ps f' 'f' { $_ -ccontains 'Far' }

### WMI class name
Test 'GWMI *process' '*process' { $_ -ccontains 'Win32_Process' }
Test 'GET-WMIOBJECT -CLASS *process' '*process' { $_ -ccontains 'Win32_Process' }

### Help comments
Test '.' '.' { $_ -ccontains '.Synopsis' -and $_ -ccontains '.Link' }
Test '.s' '.s' { $_ -ceq '.Synopsis' }
Test ' .s' '.s' { $_ -ceq '.Synopsis' }
Test "`t.s" '.s' { $_ -ceq '.Synopsis' }
Test '.d' '.d' { $_ -ceq '.Description' }
Test '.p' '.p' { $_ -ceq '.Parameter' }
Test '.i' '.i' { $_ -ceq '.Inputs' }
Test '.o' '.o' { $_ -ceq '.Outputs' }
Test '.n' '.n' { $_ -ceq '.Notes' }
Test '.e' '.e' { $_ -ceq '.Example' }
Test '.l' '.l' { $_ -ceq '.Link' }
Test '#.e' '' { $_ -ccontains '.ExternalHelp' }
Test '#  .e' '' { $_ -ccontains '.ExternalHelp' }
Test '  ##  .e' '' { $_ -ccontains '.ExternalHelp' }

### Fix of $Line.[Tab] (name is exactly 'LINE')
$Line = $Far.CommandLine
Test '' '$Line.' { $_ -ccontains '$Line.ActiveText' }

### script parameters
# script parameters; !! use $env:TEMP to avoid spaces in the path
$tmp = "$env:TEMP\" + [IO.Path]::GetRandomFileName() + '.ps1'
Set-Content $tmp -Value @'
[CmdletBinding()]
param (
$param0, $param1 = '', $param2 = $x, # comment
[xx[]]$param3 = { $z = $x * ($y + 1); $z, 1 },
[switch]$param4)
$notparam
'@
Test "$tmp -" '-' { -join $_ -ceq '-param0-param1-param2-param3-param4' }
Remove-Item $tmp

### =-patterns
Test '' '$=' { ($_ -ccontains '$LastExitCode') -and ($_ -notcontains '$Error') }
Test '' '[val=' { $_ -ccontains '[ValidateCount(#, )]' }
Test '' 'val=' { $_ -ccontains 'ValueFromPipeline = $true' -and $_ -ccontains '[ValidateCount(#, )]' }

# OK
"TabExpansion test has passed for $(([DateTime]::Now - $time1).TotalSeconds) seconds."
