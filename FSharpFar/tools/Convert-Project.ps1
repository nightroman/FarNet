
<#
.Synopsis
	Converts .fsproj to .fs.ini using ProjectCracker.

.Description
	For an input project "X.fsproj" the script writes output to "X.fs.ini" in
	the same directory as input. The existing file is recreated.

	Requires: FSharp.Compiler.Service.ProjectCracker. It has to be in the path
	or in the "packages" directory created by paket, either in the project
	directory or its parent.

.Parameter Path
		Specifies the .fsproj path.
		Default: the project in the current location.

.Parameter Parameters
		Specifies the project parameters as a hashtable.

.Example
	> Convert-Project MyProject.fsproj @{Platform = 'x64'}
#>

param(
	[Parameter()]
	[string]$Path = "C:\ROM\FarDev\Code\FSharpFar\src\FSharpFar.fsproj",
	[hashtable]$Parameters = @{}
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 1

function Get-ProjectCracker {
	$files = @(
		'FSharp.Compiler.Service.ProjectCrackerTool.exe'
		'packages\FSharp.Compiler.Service.ProjectCracker\utilities\net45\FSharp.Compiler.Service.ProjectCrackerTool.exe'
		'..\packages\FSharp.Compiler.Service.ProjectCracker\utilities\net45\FSharp.Compiler.Service.ProjectCrackerTool.exe'
	)
	foreach($file in $files) {
		if ($r = Get-Command $file -ErrorAction 0) {
			return $r
		}
	}
	throw 'Cannot find FSharp.Compiler.Service.ProjectCrackerTool.exe'
}

function Convert-Value($Value) {
	if ($Value.StartsWith($root1, [StringComparison]::OrdinalIgnoreCase)) {
		'.' + $Value.Substring($root1.Length)
	}
	elseif ($Value.StartsWith($root2, [StringComparison]::OrdinalIgnoreCase)) {
		'..' + $Value.Substring($root2.Length)
	}
	else {
		$Value
	}
}

### get project
if ($Path) {
	$Path = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Path)
	if (![System.IO.File]::Exists($Path)) {throw "Missing file: $Path"}
}
else {
	$projects = @(Get-Item -Filter *.fsproj)
	if ($projects.Count -ne 1) {throw "Please, specify the project path."}
	$Path = $projects[0].FullName
}

### convert project
$root1 = Split-Path $Path
$root2 = Split-Path $root1
Push-Location -LiteralPath $root1
try {
	$param = @('dummy')
	foreach($key in $Parameters.Keys) {
		$param += ($key, $Parameters[$key])
	}

	### run cracker
	$cracker = Get-ProjectCracker
	$isBin, $isObj = Test-Path Bin, Obj
	$json = & $cracker $Path $param
	if ($LASTEXITCODE) {throw 'ProjectCracker error.'}
	if (!$isBin) {Remove-Item Bin -Force -Recurse}
	if (!$isObj) {Remove-Item Obj -Force -Recurse}

	$data = $json | ConvertFrom-Json
	$options = $data.'Options@'

	### convert options
	$fsc = [System.Collections.Generic.List[string]]@()
	$fsi = [System.Collections.Generic.List[string]]@()
	foreach($option in $options) {
		if ($option -match '^(-+)([^:]+)(:)?(.*)') {
			$prefix = $matches[1]
			$key = $matches[2]
			$colon = $matches[3]
			$value = $matches[4]
			if ($value) {
				$value = Convert-Value $value
			}
			if ($prefix -eq '-' -and $key -eq 'r') {
				if (Test-Path -LiteralPath $value) {
					if ([System.IO.Path]::GetFileName($value) -eq 'FSharp.Core.dll') {
						# skip FSharp.Core.dll, 2+ references not permitted
					}
					else {
						$fsc.Add("reference=$value")
					}
				}
				else {
					Write-Warning "Missing reference: $value"
				}
			}
			elseif ($prefix -eq '--') {
				if ($key -eq 'out') {
				}
				elseif ($colon) {
					$fsc.Add("$key=$value")
				}
				else {
					$fsc.Add($key)
				}
			}
			else {
				Write-Warning "Unknown option: $option"
			}
		}
		else {
			$fsi.Add("load=$(Convert-Value $option)")
		}
	}

	### write output
	$out = [System.IO.Path]::ChangeExtension($Path, '.fs.ini')
	$(
		'[fsi]'
		$fsi
		'[fsc]'
		$fsc
	) | Set-Content -LiteralPath $out
}
finally {
	Pop-Location
}
