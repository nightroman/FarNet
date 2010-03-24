
<#
.SYNOPSIS
	Test TabExpansion.
	Author: Roman Kuzmin

.DESCRIPTION
	This test is for PowerShellFar in the first place. But it can be used for
	other hosts, too. Use -ErrorAction to control failures.

.NOTES
	(Get-Content -e --> -Encoding) may fail depending on the current provider,
	because it is a dynamic parameter. We test TabExpansion in here, not
	PowerShell provider cmdlets issues, so do not use Encoding.
#>

# Load TabExpansion because it is loaded on the first call only
if ($Host.Name -eq 'FarHost') {
	. "$($Psf.AppHome)\TabExpansion.ps1"
}

# Drop the cache
$global:TabExpansionCache = $null

# Runs a test and returns an error message text on failure.
# Tag: Trick: test function returns an error message if any.
# Result is piped to Write-Error, so that formal error is right in the line with Write-Error.
# Compare: if we throw in a test function then error source is the test function, more tricky.
function Test($line_, $word_, $assert_)
{
	$_ = TabExpansion $line_ $word_
	if (!(. $assert_)) {
		@"

*** TabExpansion test failed
line = $line_
word = $word_
assert = $assert_
result:
$($_ -join "`n")
"@
	}
}

# GO
$time1 = [DateTime]::Now

### static member: property, method and with prefix
Test '' '[datetime]::no' { $_ -eq '[datetime]::Now' } | Write-Error
Test '' '[datetime]::fromb' { $_ -eq '[datetime]::FromBinary(' } | Write-Error
Test '' 'Msg(([IO.File]::ref' { $_ -eq 'Msg(([IO.File]::ReferenceEquals(' } | Write-Error

### variables and members of a variable
Test '' '$hos' { $_ -eq '$Host' } | Write-Error
Test '' '@psb' { $_ -contains '@PSBoundParameters' } | Write-Error
Test '' '$global:hos' { $_ -eq '$global:Host' } | Write-Error
Test '' '$script:hos' { $_ -eq '$script:Host' } | Write-Error
Test '' '$local:hos' { $_ -eq '$local:Host' } | Write-Error
Test '' '$host.u' { $_ -eq '$Host.UI' } | Write-Error
Test '' '$host.ui.r' { $_[0] -eq '$Host.UI.RawUI' } | Write-Error
Test '' '$Host.UI.RawUI.e' { $_ -eq '$Host.UI.RawUI.Equals(' } | Write-Error
# case: base member
$xml = [xml]'<tests><test>test</test></tests>'
Test '' '$xml.ou' { $_ -eq '$xml.OuterXml'} | Write-Error
# case: adapted member
Test '' '$xml.tes' { $_ -eq '$xml.Tests'} | Write-Error

### members of a static object
Test '' '[system.datetime]::Now.h' { $_ -eq '[system.datetime]::Now.Hour' } | Write-Error

### members of a simple expression
Test '(get-date).h' '(get-date).h' { $_ -eq '(get-date).Hour' } | Write-Error
Test '(1 + 1).to' '1).to' { $_ -eq '1).ToString(' } | Write-Error

### cmdlet parameters
Test 'gc -p' '-p' { $_ -eq '-Path' } | Write-Error
Test 'Get-Content -p' '-p' { $_ -eq '-Path' } | Write-Error
Test 'ls -' '-' { $_ -contains '-Path' -and $_ -contains '-LiteralPath' } | Write-Error
Test '$e = ls -' '-' { $_ -contains '-Path' -and $_ -contains '-LiteralPath' } | Write-Error
# _091023_204251 E.g. definition of a hashtable
Test 'Name = Split-Path -l' '-l' { $_ -contains '-Leaf' -and $_ -contains '-LiteralPath' } | Write-Error

### scripts, their aliases and parameters
Test '' 'pd' { $_ -eq 'Panel-DbData-' } | Write-Error
Test '' 'Panel-Ma' { $_ -eq 'Panel-Macro-.ps1' } | Write-Error
Test 'pd -col' '-col' { $_ -eq '-Columns' } | Write-Error
Test 'Panel-DbData- -col' '-col' { $_ -eq '-Columns' } | Write-Error

# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' '-' { $_ -contains '-prm1' } | Write-Error

### function, filter, script-cmdlet parameters
# function or filter
Test 'GetScriptParameter -' '-' { $_[0] -eq '-Path' } | Write-Error
# unnamed cmdlet
Test 'man -' '-' { $_ -contains '-Name' } | Write-Error
# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' '-' { $_ -contains '-prm1' } | Write-Error

### tricky parameters
Test '# Get-Content -e' '-e' { $_ -contains '-ErrorAction' } | Write-Error
Test '>: Get-Content -e' '-e' { $_ -contains '-ErrorAction' } | Write-Error
Test '@(Get-Content $file -TotalCount ($x + $y) -e' '-e' { $_ -contains '-ErrorAction' } | Write-Error
Test '"@(Get-Content $file -TotalCount ($x + $y) -e' '-e' { $_ -contains '-ErrorAction' } | Write-Error
Test "'@(Get-Content `$file -TotalCount (`$x + `$y) -e" '-e' { $_ -contains '-ErrorAction' } | Write-Error

### drive
Test '' 'alias:epc' { $_ -eq 'alias:epcsv' } | Write-Error
Test '' '$alias:epc' { $_ -eq '$alias:epcsv' } | Write-Error
Test '' 'env:computern' { $_ -eq 'env:COMPUTERNAME' } | Write-Error
Test '' '$env:computern' { $_ -eq '$env:COMPUTERNAME' } | Write-Error
Test '' 'function:tabexp' { $_ -eq 'function:TabExpansion' } | Write-Error
Test '' '$function:tabexp' { $_ -eq '$function:TabExpansion' } | Write-Error

### scope
Test '' '$global:$host.ui.ra' { $_ -eq '$global:$host.ui.RawUI' } | Write-Error

### alias
Test '' 'gc' { $_ -eq 'Get-Content' } | Write-Error

### full path
Test '' 'c:\prog*\common' { $_ -eq 'C:\Program Files\Common Files' } | Write-Error
Test '' 'HKCU:\Software\Far2\K' { $_ -eq 'HKCU:\Software\Far2\KeyMacros' } | Write-Error

### with prefixes
Test '' ';({|$hos' { $_ -eq ';({|$Host' } | Write-Error
Test '' '!test-' { $_ -contains '!Test-Base-.ps1' } | Write-Error
Test '' '"test-' { $_ -contains '"Test-Base-.ps1' } | Write-Error
Test '' "'test-" { $_ -contains "'Test-Base-.ps1" } | Write-Error
Test '' '"$(test-' { $_ -contains '"$(Test-Base-.ps1' } | Write-Error

### function
Test '' 'Clear-H' { $_ -contains 'Clear-Host' } | Write-Error

### explicit types
Test '' '[Sy' { [string]$_ -eq '[System. [SystemException]' } | Write-Error
Test '' '[Mi' { $_ -contains '[Microsoft.' } | Write-Error
Test '' '[System.da' { $_ -contains '[System.Data.' } | Write-Error
Test '' '[System.Data.sq' { $_ -contains '[System.Data.Sql.' } | Write-Error
Test '' '[System.Data.SqlClient.SqlE' { $_[0] -eq '[System.Data.SqlClient.SqlError]' } | Write-Error

### wildcard types
Test '' '[*commandty' { $_ -contains '[System.Data.CommandType]' } | Write-Error
Test '' '[*sqlcom' { $_ -contains '[System.Data.SqlClient.SqlCommand]' } | Write-Error

### types for New-Object
Test 'NEW-OBJECT System.da' 'System.da' { $_ -contains 'System.Data.' } | Write-Error
Test 'NEW-OBJECT  -TYPENAME  System.da' 'System.da' { $_ -contains 'System.Data.' } | Write-Error
Test 'NEW-OBJECT   System.Data.SqlClient.SqlE' 'System.Data.SqlClient.SqlE' { $_[0] -eq 'System.Data.SqlClient.SqlError' } | Write-Error

### WMI
Test '' 'win32*sc*j' { $_ -eq 'Win32_ScheduledJob' } | Write-Error

### Module name
Test 'IMPORT-MODULE b' 'b' { $_ -contains 'BitsTransfer' } | Write-Error
Test 'IPMO b' 'b' { $_ -contains 'BitsTransfer' } | Write-Error
Test 'IMPORT-MODULE -NAME b' 'b' { $_ -contains 'BitsTransfer' } | Write-Error
Test 'IPMO -NAME b' 'b' { $_ -contains 'BitsTransfer' } | Write-Error

### Process name
Test 'GET-PROCESS f' 'f' { $_ -contains 'Far' } | Write-Error
Test 'ps f' 'f' { $_ -contains 'Far' } | Write-Error

### Help comments
Test '.' '.' { $_ -contains '.SYNOPSIS' -and $_ -contains '.LINK' } | Write-Error
Test '.s' '.s' { $_ -eq '.SYNOPSIS' } | Write-Error
Test ' .s' '.s' { $_ -eq '.SYNOPSIS' } | Write-Error
Test "`t.s" '.s' { $_ -eq '.SYNOPSIS' } | Write-Error
Test '.d' '.d' { $_ -eq '.DESCRIPTION' } | Write-Error
Test '.p' '.p' { $_ -eq '.PARAMETER' } | Write-Error
Test '.i' '.i' { $_ -eq '.INPUTS' } | Write-Error
Test '.o' '.o' { $_ -eq '.OUTPUTS' } | Write-Error
Test '.n' '.n' { $_ -eq '.NOTES' } | Write-Error
Test '.e' '.e' { $_ -eq '.EXAMPLE' } | Write-Error
Test '.l' '.l' { $_ -eq '.LINK' } | Write-Error

### Fix of $Line.[Tab] (name is exactly 'LINE')
$Line = $Far.CommandLine
Test '' '$Line.' { $_ -contains '$Line.ActiveText' } | Write-Error

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
Test "$tmp -" '-' { -join $_ -eq '-param0-param1-param2-param3-param4' } | Write-Error
Remove-Item $tmp

# Drop the cache
Remove-Variable -Scope global TabExpansionCache

# OK
"TabExpansion test has passed for $(([DateTime]::Now - $time1).TotalSeconds) seconds."

<#
Bug [_080510_023705]: V2 CTP2 or CTP1: 'param' is parsed as 'command' in script-cmdlets
To watch: perhaps it is a bug to be fixed in V2 RTM.
090112: V2 CTP3 - fixed
#>
