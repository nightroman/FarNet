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

Assert-Far ($env:MERGE)
$e = try {Start-Transcript env:MERGE} catch {$_}
Assert-Far ($e -clike '*The specified path is not a file.*')

### Bad path

$e = try {Start-Transcript C:\TEMP\z*} catch {$_}
Assert-Far ($e -clike '*Illegal characters in path.*')

### Not started

# Stop-Transcript fails, Show-FarTranscript may work with the last
$e = try {Stop-Transcript} catch {$_}
Assert-Far ($e -clike '*Transcription has not been started.*')

### Start

$log = "$env:TEMP\transcript.txt"
[System.IO.File]::Delete($log)

$r = Start-Transcript $log
Assert-Far @(
	Test-Path -LiteralPath $log
	$r -ceq "Transcript started, output file is $log"
)

### Already started

$e = try {Start-Transcript} catch {$_}
Assert-Far ($e -clike '*Transcription has already been started.*')

### Stop

$r = Stop-Transcript
Assert-Far ($r -ceq "Transcript stopped, output file is $log")
Remove-Item -LiteralPath $log
$Error.Clear()
