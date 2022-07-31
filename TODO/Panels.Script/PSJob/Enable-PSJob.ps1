<#
.Synopsis
	Enables PowerShell jobs in PowerShellFar.

.Description
	This is a temporary hack. PowerShell Core host requires pwsh and uses it
	for jobs. But there is no way to configure this, pwsh is expected in the
	particular folder. So this script copies existing pwsh to this folder:
	%FARHOME%\FarNet\Modules\PowerShellFar\runtimes\win\lib\net6.0

.Parameter PwshPath
		Specifies the path to pwsh.exe. You may omit this parameter if pwsh is
		in the path. Make sure pwsh has the same version as PowerShellFar.
		Use $PSVersionTable to check versions.

.Parameter FarHome
		Specifies the Far Manager home and uses $env:FARHOME as the default.
#>

[CmdletBinding()]
param(
	[string]
	$PwshPath
	,
	[string]
	$FarHome = $env:FARHOME
)

$ErrorActionPreference = 1
trap { Write-Error $_ }

### resolve PwshPath
if (!$PwshPath) {
	$r = Get-Command pwsh.exe -CommandType Application -ErrorAction 0
	if (!$r) {
		throw "Cannot find 'pwsh.exe'. Add it to the path or specify as '-PwshPath'."
	}
	$PwshPath = $r.Source
}

### resolve FarHome
if (!$FarHome) {
	$FarHome = 'C:\Bin\Far\x64'
}
if (!(Test-Path -LiteralPath $FarHome)) {
	throw "Please set %FARHOME% or specify as '-FarHome'."
}

### resolve target
$targetDir = "$FarHome\FarNet\Modules\PowerShellFar\runtimes\win\lib\net6.0"
if (!(Test-Path -LiteralPath $targetDir)) {
	throw "Cannot find the folder '$targetDir'."
}
$targetPath = Join-Path $targetDir pwsh.exe
if (Test-Path -LiteralPath $targetPath) {
	Remove-Item -LiteralPath $targetPath
}

### copy pwsh
Copy-Item -LiteralPath $PwshPath -Destination $targetDir
