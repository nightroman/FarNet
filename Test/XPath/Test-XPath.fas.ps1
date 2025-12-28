
job {
	### Test the 'equals' function and a custom variable
	$explorer = [PowerShellFar.ItemExplorer]$env:FarNetCode
	$search = [FarNet.Tools.SearchFileCommand]$explorer
	$search.XVariables.Add('Name', 'history.txt')
	# find 'license' in 1 level folders
	$search.XPath = @'
/*/File[equals(@Name, $Name)]
'@
	$res = $search.Invoke()
	Assert-Far 14 -eq @($res).Count
}

job {
	### Test the 'is-match' function
	$explorer = [PowerShellFar.ItemExplorer]$env:FarNetCode
	$search = [FarNet.Tools.SearchFileCommand]$explorer

	# gets *.csproj which folders have *.lua
	$search.XPath = @'
//File
[
	is-match(@Name, '(?i)\.csproj$')
	and
	../File[is-match(@Name, '(?i)\.lua$')]
]
'@

	$r = $search.Invoke() | Sort-Object name | Join-String -Separator /
	Assert-Far $r -eq 'RightControl.csproj/RightWords.csproj/Script.csproj/Vessel.csproj'
}
