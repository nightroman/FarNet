Set-Alias Start-Transcript Start-FarTranscript
Set-Alias Stop-Transcript Stop-FarTranscript

<#
.Synopsis
	PSF Clear-Host.
#>
function Clear-Host {
	$Far.UI.Clear()
}

<#
.Synopsis
	PSF Get-Help | more.
#>
function help {
	$Far.UI.ShowUserScreen()
	Get-Help @args | more
	$Far.UI.SaveUserScreen()
}

<#
.Synopsis
	PSF Get-History.
#>
function Get-History([Parameter()][int]$Count = [int]::MaxValue) {
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
function Show-FarTranscript([Parameter()][switch]$Internal) {
	trap {$PSCmdlet.ThrowTerminatingError($_)}
	[PowerShellFar.Transcript]::ShowTranscript($Internal)
}

<#
.ForwardHelpTargetName Stop-Transcript
.ForwardHelpCategory Cmdlet
#>
function Stop-FarTranscript {
	[CmdletBinding()]
	param()
	trap {$PSCmdlet.ThrowTerminatingError($_)}
	[PowerShellFar.Transcript]::StopTranscript($false)
}

<#
.ForwardHelpTargetName Start-Transcript
.ForwardHelpCategory Cmdlet
#>
function Start-FarTranscript {
	[CmdletBinding(DefaultParameterSetName='Path')]
	param(
		[Parameter(ParameterSetName='Path', Position=0)]
		[ValidateNotNullOrEmpty()]
		[Alias('LiteralPath')]
		[Alias('PSPath')]
		[string]$Path,
		[Parameter(ParameterSetName='Dir', Mandatory=1)]
		[string]$OutputDirectory,
		[switch]$Append,
		[switch]$IncludeInvocationHeader,
		[switch]$Force,
		[Alias('NoOverwrite')]
		[switch]$NoClobber,
		[switch]$UseMinimalHeader
	)
	trap {$PSCmdlet.ThrowTerminatingError($_)}
	$a = [PowerShellFar.Transcript+Args]@{
		Append=$Append
		IncludeInvocationHeader=$IncludeInvocationHeader
		Force=$Force
		NoClobber=$NoClobber
		UseMinimalHeader=$UseMinimalHeader
	}
	if ($Path) {$a.Path = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path)}
	elseif ($OutputDirectory) {$a.OutputDirectory = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($OutputDirectory)}
	[PowerShellFar.Transcript]::StartTranscript($a)
}
