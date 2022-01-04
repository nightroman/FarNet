<#
.Synopsis
	Test assorted features.
#>

# Test clipboard methods
function Test-Clipboard
{
	$s1 = "йцукен"
	$Far.CopyToClipboard($s1)
	$s2 = $Far.PasteFromClipboard()
	Assert-Far ($s1 -eq $s2)
}

# Test version
function Test-Version
{
	# assume: 3.0.dddd.0
	Assert-Far ("$($Far.FarVersion)" -match '^3\.0\.\d{4}\.0$')
}

# Test PSF module paths, it was tricky to get this result, let's keep it checked. [_100127_182335]
function Test-ModulePath
{
	$paths = $env:PSModulePath -split ';'
	Assert-Far @(
		$paths.Count -ge 3
		$paths[0] -eq "$($Psf.AppHome)\Modules"
		$paths -contains "$HOME\Documents\WindowsPowerShell\Modules"
	)
	# other path names depend on x86 x64 and v4.0 has extra path in Program Files
}

Test-Clipboard
Test-ModulePath
Test-Version
