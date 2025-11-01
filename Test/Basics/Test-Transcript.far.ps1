<#
.Synopsis
	Test transcript basics.
#>

# force stop
[PowerShellFar.Transcript]::StopTranscript($true)

### Bad $Transcript

if ($Transcript? = Test-Path Variable:\Global:Transcript) {
	$Transcript0 = Get-Variable Transcript -Scope Global -ValueOnly
}

$global:Transcript = 1
try { throw Start-Transcript }
catch { Assert-Far "$_" -eq '$Transcript value is not a string.' }

if ($Transcript?) {
	$global:Transcript = $Transcript0
}
else {
	Remove-Variable Transcript -Scope Global
}

### Not a file

try { throw Start-Transcript $HOME }
catch { Assert-Far ($_ -clike '*The specified path is not a file.*') }

### Bad path

try { throw Start-Transcript C:\TEMP\z* }
catch { Assert-Far ($_ -clike 'The filename, directory name, or volume label syntax is incorrect.*') }

### Not started

# Stop-Transcript fails, Show-FarTranscript may work with the last
try { throw Stop-Transcript }
catch { Assert-Far "$_" -eq 'The host is not currently transcribing.' }

### Start 1, 2

$log1 = "$env:TEMP\transcript1.txt"
$log2 = "$env:TEMP\transcript2.txt"
Remove-Item $log1, $log2 -ea Ignore

$r = Start-Transcript $log1
Assert-Far (Test-Path $log1)
Assert-Far $r -eq "Transcript started, output file is $log1"
Assert-Far $r.Path -eq $log1

$r = Start-Transcript -LiteralPath $log2 #! alias
Assert-Far (Test-Path $log2)
Assert-Far $r -eq "Transcript started, output file is $log2"
Assert-Far $r.Path -eq $log2

### Stop 1, 2

$r = Stop-Transcript
Assert-Far $r -eq "Transcript stopped, output file is $log2"
Assert-Far $r.Path -eq $log2

$r = Stop-Transcript
Assert-Far $r -eq "Transcript stopped, output file is $log1"
Assert-Far $r.Path -eq $log1

Remove-Item $log1, $log2
$Error.Clear()
