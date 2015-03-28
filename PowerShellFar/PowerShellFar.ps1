
<#
.Synopsis
	The internal profile.
	Author: Roman Kuzmin
#>

# Ignore errors
trap { continue }

<#
.Synopsis
	PSF Clear-Host.
#>
function Clear-Host {
	$Far.UI.Clear()
}

<#
.Synopsis
	PSF Get-History.
#>
function Get-History(
	[Parameter()][int]
	$Count = [int]::MaxValue
)
{
	$Psf.GetHistory($Count)
}

<#
.Synopsis
	PSF Invoke-History.
#>
function Invoke-History {
	$Psf.ShowHistory()
}

<#
.Synopsis
	Shows transcribed command output in the external or internal viewer.
.Parameter Internal
		Tells to show in the internal viewer.
#>
function Show-FarTranscript(
	[Parameter()]
	[switch]$Internal
)
{
	try {
		[PowerShellFar.Zoo]::ShowTranscript($Internal)
	}
	catch {
		Write-Error -ErrorAction Stop $_
	}
}

<#
.ForwardHelpTargetName Stop-Transcript
.ForwardHelpCategory Cmdlet
#>
function Stop-FarTranscript {
	[CmdletBinding()]
	param()
	try {
		[PowerShellFar.Zoo]::StopTranscript($false)
	}
	catch {
		Write-Error -ErrorAction Stop $_
	}
}
Set-Alias Stop-Transcript Stop-FarTranscript

<#
.ForwardHelpTargetName Start-Transcript
.ForwardHelpCategory Cmdlet
#>
function Start-FarTranscript {
	param(
		[Parameter(Position=0)]
		[Alias('Path')]
		[Alias('PSPath')]
		[ValidateNotNullOrEmpty()]
		[string]
		$LiteralPath,
		[switch]
		$Append,
		[switch]
		$Force,
		[Alias('NoOverwrite')]
		[switch]
		$NoClobber
	)
	try {
		if (!$LiteralPath) {
			if ($path = $PSCmdlet.GetVariableValue('global:Transcript')) {
				if ($path -isnot [string]) {throw '$Transcript value is not a string.'}
				$LiteralPath = $path
			}
		}
		if ($LiteralPath) {
			if (Test-Path -LiteralPath $LiteralPath) {
				$item = Get-Item -LiteralPath $LiteralPath -ErrorAction Stop
				if ($item -isnot [System.IO.FileInfo]) {throw 'The specified path is not a file.'}
				$LiteralPath = $item.FullName
			}
			else {
				$LiteralPath = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($LiteralPath)
			}
		}
		[PowerShellFar.Zoo]::StartTranscript($LiteralPath, $Append, $Force, $NoClobber)
	}
	catch {
		Write-Error -ErrorAction Stop $_
	}
}
Set-Alias Start-Transcript Start-FarTranscript
