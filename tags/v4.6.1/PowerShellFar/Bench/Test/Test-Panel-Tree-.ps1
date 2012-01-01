
<#
.Synopsis
	Test panel mixed items on TreePanel
	Author: Roman Kuzmin

.Description
	Points of interest:
	- root items of different types: Services, Processes, Provider items
	- root items can be optionally pre-expanded: $p.AddRoot($r, $true)
	- nested levels of different types: Providers\Drives\Items\..
	- unlimited recursive expansion: $t.Fill = $0.Fill
	- items are not expandable is .Fill is not set
	- using .Data for later use of stored objects
	- using .Description for extra information
	- using custom handler of <Enter> event
#>

# Explorer
$explorer = New-Object PowerShellFar.TreeExplorer

# Add a root for services (not yet expanded)
$r = $explorer.RootFiles.Add()
$r.Description = 'Services and their statuses'
$r.Name = 'Services'
$r.Fill = {
	param($0)
	foreach($service in (Get-Service)) {
		$t = $0.ChildFiles.Add()
		$t.Data = $service
		$t.Description = $service.Status
		$t.Name = $service.ServiceName
	}
}

# Add a root for processes (not yet expanded)
$r = $explorer.RootFiles.Add()
$r.Description = 'Processes and their paths'
$r.Name = 'Processes'
$r.Fill = {
	param($0)
	foreach($process in (Get-Process)) {
		$t = $0.ChildFiles.Add()
		$t.Data = $process
		$t.Description = $process.Path
		$t.Name = $process.ProcessName
	}
}

# Add a root for Providers\Drives\Items\.. (pre-expanded)
$r = $explorer.RootFiles.Add()
$r.Description = 'Providers\Drives\Items\..'
$r.Name = 'Providers'
$r.Fill = {
	param($0)
	foreach($provider in (Get-PSProvider)) {
		$t = $0.ChildFiles.Add()
		$t.Data = $provider
		$t.Description = $provider.Drives
		$t.Name = $provider.Name
		$t.Fill = {
			param($0)
			foreach($drive in $0.Data.Drives) {
				$t = $0.ChildFiles.Add()
				$t.Data = $drive.Name + ':\'
				$t.Name = $drive.Name + ':'
				$t.Fill = {
					param($0)
					foreach($item in (Get-ChildItem -LiteralPath $0.Data -ea 0)) {
						$t = $0.ChildFiles.Add()
						$t.Data = $item.PSPath
						$t.Description = $item.Description
						if ($item.PSChildName) {
							$t.Name = $item.PSChildName
						}
						else {
							$t.Name = $item.Name
						}
						if ($item.PSIsContainer) {
							$t.Fill = $0.Fill
						}
					}
				}
			}
		}
	}
}
$r.Expand()

# New TreePanel
$panel = New-Object PowerShellFar.TreePanel $explorer
$panel.Title = 'Test-Panel-Tree-.ps1'

# On <Enter>: open a child member panel for a current object
# This is the same as [CtrlPgDn], we do it for testing only.
$panel.AsOpenFile = {
	param($0, $_)
	$_.File | Open-FarPanel -Title "$($_.File.Name) opened by [Enter]" -AsChild
}

# Go
$panel.Open()
