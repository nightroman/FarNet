
<#
.SYNOPSIS
	Test TabExpansion.
	Author: Roman Kuzmin

.DESCRIPTION
	This test is for the PowerShellFar development environment. It may fail in
	a different environment. But with adjustments most of the test should work.

.NOTES
	(Get-Content -e --> -Encoding) may fail depending on the current provider,
	because it is a dynamic parameter. We test TabExpansion in here, not
	PowerShell provider cmdlets issues, so do not use Encoding.
#>

# Ensure TabExpansion is loaded
if ($Host.Name -eq 'FarHost') {
	. "$($Psf.AppHome)\TabExpansion.ps1"
}

# Set location in here, we assume and use some files
Set-Location -LiteralPath (Split-Path $MyInvocation.MyCommand.Path)

# Invokes TabExpansion and tests the results.
function Test([Parameter()]$line_, $word_, $assert_)
{
	$_ = TabExpansion $line_ $word_
	if (!(. $assert_)) {
		$PSCmdlet.ThrowTerminatingError((New-Object System.Management.Automation.ErrorRecord ([Exception]@"
*** TabExpansion test failed
line = $line_
word = $word_
result:
$($_ -join "`n")
"@), '', 0, $null))
	}
}

# GO
$time1 = [DateTime]::Now

### static member: property, method and with prefix
Test '' '[datetime]::no' { $_ -eq '[datetime]::Now' }
Test '' '[datetime]::fromb' { $_ -eq '[datetime]::FromBinary(' }
Test '' 'Msg(([IO.File]::ref' { $_ -eq 'Msg(([IO.File]::ReferenceEquals(' }

### variables and members of a variable
Test '' '$hos' { $_ -eq '$Host' }
Test '' '@psb' { $_ -contains '@PSBoundParameters' }
Test '' '$global:hos' { $_ -eq '$global:Host' }
Test '' '$script:hos' { $_ -eq '$script:Host' }
Test '' '$local:hos' { $_ -eq '$local:Host' }
Test '' '$host.u' { $_ -eq '$Host.UI' }
Test '' '$host.ui.r' { $_[0] -eq '$Host.UI.RawUI' }
Test '' '$Host.UI.RawUI.e' { $_ -eq '$Host.UI.RawUI.Equals(' }
# case: base member
$xml = [xml]'<tests><test>test</test></tests>'
Test '' '$xml.ou' { $_ -eq '$xml.OuterXml'}
# case: adapted member
Test '' '$xml.tes' { $_ -eq '$xml.Tests'}

### members of a static object
Test '' '[system.datetime]::Now.h' { $_ -eq '[system.datetime]::Now.Hour' }

### members of a simple expression
Test '(get-date).h' '(get-date).h' { $_ -eq '(get-date).Hour' }
Test '(1 + 1).to' '1).to' { $_ -eq '1).ToString(' }
Test "('string').le" "('string').le" { $_ -eq "('string').Length" }

### cmdlet parameters
Test 'gc -p' '-p' { $_ -eq '-Path' }
Test 'Get-Content -p' '-p' { $_ -eq '-Path' }
Test 'ls -' '-' { $_ -contains '-Path' -and $_ -contains '-LiteralPath' }
Test '$e = ls -' '-' { $_ -contains '-Path' -and $_ -contains '-LiteralPath' }
# _091023_204251 E.g. definition of a hashtable
Test 'Name = Split-Path -l' '-l' { $_ -contains '-Leaf' -and $_ -contains '-LiteralPath' }

### scripts, their aliases and parameters
Test '' 'pd' { $_ -eq 'Panel-DbData-' }
Test '' 'Panel-Ma' { $_ -eq 'Panel-Macro-.ps1' }
Test 'pd -col' '-col' { $_ -eq '-Columns' }
Test 'Panel-DbData- -col' '-col' { $_ -eq '-Columns' }

# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' '-' { $_ -contains '-prm1' }

### function, filter, script-cmdlet parameters
# function or filter
Test 'GetScriptParameter -' '-' { $_[0] -eq '-Path' }
# unnamed cmdlet
Test 'man -' '-' { $_ -contains '-Name' }
# advanced function
function test-me { param ([Parameter(Position = 0, Mandatory = $true)]$prm1, $prm2) {} }
Test 'test-me -' '-' { $_ -contains '-prm1' }

### tricky parameters
Test '# Get-Content -e' '-e' { $_ -contains '-ErrorAction' }
Test '>: Get-Content -e' '-e' { $_ -contains '-ErrorAction' }
Test '@(Get-Content $file -TotalCount ($x + $y) -e' '-e' { $_ -contains '-ErrorAction' }
Test '"@(Get-Content $file -TotalCount ($x + $y) -e' '-e' { $_ -contains '-ErrorAction' }
Test "'@(Get-Content `$file -TotalCount (`$x + `$y) -e" '-e' { $_ -contains '-ErrorAction' }

### drive
Test '' 'alias:epc' { $_ -eq 'alias:epcsv' }
Test '' '$alias:epc' { $_ -eq '$alias:epcsv' }
Test '' 'env:computern' { $_ -eq 'env:COMPUTERNAME' }
Test '' '$env:computern' { $_ -eq '$env:COMPUTERNAME' }
Test '' 'function:tabexp' { $_ -eq 'function:TabExpansion' }
Test '' '$function:tabexp' { $_ -eq '$function:TabExpansion' }

### scope
Test '' '$global:$host.ui.ra' { $_ -eq '$global:$host.ui.RawUI' }

### alias
Test '' 'gc' { $_ -eq 'Get-Content' }

### full path
Test '' 'c:\prog*\common' { $_ -eq 'C:\Program Files\Common Files' }
Test '' 'HKCU:\Software\Far2\K' { $_ -eq 'HKCU:\Software\Far2\KeyMacros' }

### with prefixes
Test '' ';({|$hos' { $_ -eq ';({|$Host' }
Test '' '!test-' { $_ -contains '!Test-Base-.ps1' }
Test '' '"test-' { $_ -contains '"Test-Base-.ps1' }
Test '' "'test-" { $_ -contains "'Test-Base-.ps1" }
Test '' '"$(test-' { $_ -contains '"$(Test-Base-.ps1' }

### function
Test '' 'Clear-H' { $_ -contains 'Clear-Host' }

### namespace and type names

# drop the cache and add used types before the tests
$global:TabExpansionCache = $null
Add-Type -AssemblyName System.Windows.Forms

# explicit namespace and type names
Test '' '[Sy' { [string]$_ -eq '[System. [SystemException]' }
Test '' '[Mi' { $_ -contains '[Microsoft.' }
Test '' '[System.da' { $_ -contains '[System.Data.' }
Test '' '[System.Data.sq' { $_ -contains '[System.Data.Sql.' }
Test '' '[System.Data.SqlClient.SqlE' { $_[0] -eq '[System.Data.SqlClient.SqlError]' }

# wildcard namespace and type names
Test '' '[*commandty' { $_ -contains '[System.Data.CommandType]' }
Test '' '[*sqlcom' { $_ -contains '[System.Data.SqlClient.SqlCommand]' }

# New-Object namespace and type names
Test 'NEW-OBJECT System.da' 'System.da' { $_ -contains 'System.Data.' }
Test 'NEW-OBJECT  -TYPENAME  System.da' 'System.da' { $_ -contains 'System.Data.' }
Test 'NEW-OBJECT   System.Data.SqlClient.SqlE' 'System.Data.SqlClient.SqlE' { $_[0] -eq 'System.Data.SqlClient.SqlError' }
Test 'New-Object System.Data.SqlClient.' 'System.Data.SqlClient.' { $_.Count -gt 1 }
#_100728_121000
# Weird: $set1.Keys is kind of $null for System.Windows.Forms.[Tab], so we use there GetEnumerator()
Test 'New-Object System.Windows.Forms.' 'System.Windows.Forms.' { $_.Count -gt 1 }

### Module name
Test 'IMPORT-MODULE b' 'b' { $_ -contains 'BitsTransfer' }
Test 'IPMO b' 'b' { $_ -contains 'BitsTransfer' }
Test 'IMPORT-MODULE -NAME b' 'b' { $_ -contains 'BitsTransfer' }
Test 'IPMO -NAME b' 'b' { $_ -contains 'BitsTransfer' }

### Process name
Test 'GET-PROCESS f' 'f' { $_ -contains 'Far' }
Test 'ps f' 'f' { $_ -contains 'Far' }

### WMI class name
Test 'GWMI *process' '*process' { $_ -contains 'Win32_Process' }
Test 'GET-WMIOBJECT -CLASS *process' '*process' { $_ -contains 'Win32_Process' }

### Help comments
Test '.' '.' { $_ -ccontains '.Synopsis' -and $_ -contains '.Link' }
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
Test '' '$Line.' { $_ -contains '$Line.ActiveText' }

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
Test "$tmp -" '-' { -join $_ -eq '-param0-param1-param2-param3-param4' }
Remove-Item $tmp

### #-patterns
Test '' '$#' { ($_ -contains '$LASTEXITCODE') -and ($_ -notcontains '$Error') }
Test '' '[val#' { $_ -contains '[ValidateCount(#, )]' }
Test '' 'val#' { $_ -contains 'ValueFromPipeline = $true' -and $_ -notcontains '[ValidateCount(#, )]' }
# Drop the cache
Remove-Variable -Scope global TabExpansionCache

# OK
"TabExpansion test has passed for $(([DateTime]::Now - $time1).TotalSeconds) seconds."

<#
Bug [_080510_023705]: V2 CTP2 or CTP1: 'param' is parsed as 'command' in script-cmdlets
To watch: perhaps it is a bug to be fixed in V2 RTM.
090112: V2 CTP3 - fixed
#>
