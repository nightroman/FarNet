<#
	Covers standard explorers.
	https://github.com/nightroman/FarNet/issues/13
#>

job {
	$explorer = New-Object PowerShellFar.PowerExplorer '61e548ae-3d83-488e-a18f-4633ea7b6b32' -Property @{
		AsGetFiles = {
			$r = New-Object FarNet.SetFile
			$r.Name = 'bug13'
			$r
		}
		AsExploreParent = {
			$Data.AsExploreParent = 'called'
			$Far.Panel.Close()
		}
		AsEnterPanel = {
			param($0, $_)
			$_.DotsMode = 'Dots'
		}
	}
	$explorer.CreatePanel().Open()
}

macro 'Keys"Enter" -- enter on dots'

job {
	Assert-Far $Data.AsExploreParent -eq 'called'
}
