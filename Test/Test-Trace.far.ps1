
$Log = "C:\Temp\test.trace.log"

# It used to be cmdlet
function Trace-Far
(
	[Parameter(Position = 0, Mandatory = $true, HelpMessage = "Event ID.")]
	[int]$Id
	,
	[Parameter(Position = 1, Mandatory = $true, HelpMessage = "Event type of the trace data.")]
	[Diagnostics.TraceEventType]$EventType
	,
	[Parameter(Position = 2, HelpMessage = "Event message or format string with -Data.")]
	$Format
	,
	[Parameter(Position = 3, HelpMessage = "Data for TraceEvent() or TraceData().")]
	$Data
)
{
	if ($Format -eq $null) {
		[FarNet.Log]::Source.TraceData($EventType, $Id, $Data)
	}
	elseif ($Data -eq $null) {
		[FarNet.Log]::Source.TraceEvent($EventType, $Id, $Format)
	}
	else {
		[FarNet.Log]::Source.TraceEvent($EventType, $Id, $Format, $Data)
	}
}

function Remove-Log
{
	if (Test-Path $Log) { Remove-Item $Log }
}

function New-Listener
{
	New-Object Diagnostics.TextWriterTraceListener $Log, 'Test-TraceFar'
}

function Test-TraceLevel($Id, $TestLevel)
{
	# set new level
	$Source.Switch.Level = $TestLevel

	# test all events
	Trace-Far $Id Critical Critical
	Trace-Far $Id Error Error
	Trace-Far $Id Warning Warning
	Trace-Far $Id Information Information
	Trace-Far $Id Verbose Verbose
	Trace-Far $Id Start Start
	Trace-Far $Id Stop Stop
	Trace-Far $Id Suspend Suspend
	Trace-Far $Id Resume Resume
	Trace-Far $Id Transfer Transfer

	# restore level
	$Source.Switch.Level = $Level
}

### Simple fixtures
# Note: some were actual for the Trace-Far cmdlet

# empty line
[Diagnostics.Trace]::WriteLine('')

#! pipeline binding used to fails
[Diagnostics.Trace]::WriteLine('----- Test of Trace and FarNet trace source -----')

### Get the trace source

$Source = [FarNet.Log]::Source
$Level = $Source.Switch.Level
[Diagnostics.Trace]::WriteLine("Switch.Level = $Level")
for($1 = 0; $1 -lt $Source.Listeners.Count; ++$1) {
	$Listener = $Source.Listeners[$1]
	[Diagnostics.Trace]::WriteLine("Switch.Listeners[$1] = $($Listener)")
}
Assert-Far @($Source.Switch.ShouldTrace('Error'))

### Test Trace

Remove-Log
$Listener = New-Listener
$null = [Diagnostics.Trace]::Listeners.Add($Listener)

Trace-Far 0 Error 'This text should not be in the log file'
[Diagnostics.Trace]::WriteLine('Trace message')
[Diagnostics.Trace]::WriteLine('Trace message', 'Trace category')
[Diagnostics.Trace]::TraceError('Trace error')
[Diagnostics.Trace]::TraceWarning('Trace warning')
[Diagnostics.Trace]::TraceInformation('Trace information')

[Diagnostics.Trace]::Listeners.Remove('Test-TraceFar')
$Listener.Close()

$text = [IO.File]::ReadAllText($Log)
Assert-Far ($text -eq @'
Trace message
Trace category: Trace message
Far.exe Error: 0 : Trace error
Far.exe Warning: 0 : Trace warning
Far.exe Information: 0 : Trace information

'@)
Remove-Log

### Test FarNet trace source

Remove-Log
$Listener = New-Listener
$null = $Source.Listeners.Add($Listener)

[Diagnostics.Trace]::WriteLine('This text should not be in the log file')
Test-TraceLevel 1 Warning
Test-TraceLevel 2 All
Assert-Far ($Level -eq $Source.Switch.Level)

### TraceData
Trace-Far 3 Error -Data 1234
Trace-Far 3 Error -Data 1234, 'text'
Trace-Far 3 Error -Data 'text', 1234

### TraceEvent message and format message
Trace-Far 4 Error 'Simple text'
Trace-Far 4 Error 'Formatted {0} value' 1
Trace-Far 4 Error 'Formatted values: "{0}" and "{1}"' 1234, 'text'

$Source.Listeners.Remove('Test-TraceFar')
$Listener.Close()

$text = [IO.File]::ReadAllText($Log)
Assert-Far ($text -eq @'
FarNet Critical: 1 : Critical
FarNet Error: 1 : Error
FarNet Warning: 1 : Warning
FarNet Critical: 2 : Critical
FarNet Error: 2 : Error
FarNet Warning: 2 : Warning
FarNet Information: 2 : Information
FarNet Verbose: 2 : Verbose
FarNet Start: 2 : Start
FarNet Stop: 2 : Stop
FarNet Suspend: 2 : Suspend
FarNet Resume: 2 : Resume
FarNet Transfer: 2 : Transfer
FarNet Error: 3 : 1234
FarNet Error: 3 : 1234, text
FarNet Error: 3 : text, 1234
FarNet Error: 4 : Simple text
FarNet Error: 4 : Formatted 1 value
FarNet Error: 4 : Formatted values: "1234" and "text"

'@)
Remove-Log

### Mixed output

Remove-Log
$Listener = New-Listener
$null = [Diagnostics.Trace]::Listeners.Add($Listener)
$null = $Source.Listeners.Add($Listener)

[Diagnostics.Trace]::WriteLine('By Trace')
[Diagnostics.Trace]::TraceError('By Trace')
Trace-Far 0 Error 'By TraceSource'

[Diagnostics.Trace]::Listeners.Remove('Test-TraceFar')
$Source.Listeners.Remove('Test-TraceFar')
$Listener.Close()

$text = [IO.File]::ReadAllText($Log)
Assert-Far ($text -eq @'
By Trace
Far.exe Error: 0 : By Trace
FarNet Error: 0 : By TraceSource

'@)
Remove-Log
