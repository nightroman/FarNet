<#
.Synopsis
	Test transcript basics.
#>

Assert-Far (!$Error) -Message 'Please clear $Errors.'

# force stop
[PowerShellFar.Zoo]::StopTranscript($true)

### Bad $Transcript

if ($Transcript? = Test-Path Variable:\Global:Transcript) {
	$Transcript0 = Get-Variable Transcript -Scope Global -ValueOnly
}

$global:Transcript = 1
$e = try {Start-Transcript} catch {$_}
Assert-Far ($e -clike '*$Transcript value is not a string.*')

if ($Transcript?) {
	$global:Transcript = $Transcript0
}
else {
	Remove-Variable Transcript -Scope Global
}

### Not a file

$e = try {Start-Transcript $HOME} catch {$_}
Assert-Far ($e -clike '*The specified path is not a file.*')

### Bad path

$e = try {Start-Transcript C:\TEMP\z*} catch {$_}
Assert-Far ($e -clike 'The filename, directory name, or volume label syntax is incorrect.*')

### Not started

# Stop-Transcript fails, Show-FarTranscript may work with the last
$e = try {Stop-Transcript} catch {$_}
Assert-Far ($e -clike '*Transcription has not been started.*')

### Start

$log = "$env:TEMP\transcript.txt"
[System.IO.File]::Delete($log)

$r = Start-Transcript $log
Assert-Far (Test-Path -LiteralPath $log)
Assert-Far $r -eq "Transcript started, output file is $log"
Assert-Far $r.Path -eq $log

### Already started

$e = try {Start-Transcript} catch {$_}
Assert-Far ($e -clike '*Transcription has already been started.*')

### Stop

$r = Stop-Transcript
Assert-Far $r -eq "Transcript stopped, output file is $log"
Assert-Far $r.Path -eq $log
Remove-Item -LiteralPath $log
$Error.Clear()
