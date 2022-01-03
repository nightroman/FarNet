<#
.Synopsis
	Tests TabExpansion2 profile issues.
#>

job {
	if ($global:Error) {throw 'Please remove errors.'}

	# add the root to the path
	$thePath = $env:Path
	if (!($thePath.Contains($PSScriptRoot))) {
		$env:Path = "$PSScriptRoot;$thePath"
	}

	# load TabExpansion2, it will cause loading of profiles
	$TabExpansion2 = "$($Psf.AppHome)\TabExpansion2.ps1"
	& $TabExpansion2

	# call TabExpansion2, it loads profiles and then works
	$r = (TabExpansion2 ls 2).CompletionMatches[0].CompletionText
	Assert-Far $r -eq 'Get-ChildItem'

	# test errors
	$global:Error.Reverse()
	Assert-Far ($global:Error[0] -like 'TabExpansion2: Unexpected output. Profile: *\Bad.ArgumentCompleters.ps1')
	Assert-Far ($global:Error[1] -like 'TabExpansion2: Invalid result. Input processor:*### Issue - invalid input processor result*42*')
	Assert-Far ($global:Error[2] -like 'TabExpansion2: Unexpected output. Result processor:*### Issue - unexpected result processor output*42*')
	Assert-Far $global:Error.Count -eq 3

	# OK
	$global:Error.Clear()
	$env:Path = $thePath
	& $TabExpansion2
}
