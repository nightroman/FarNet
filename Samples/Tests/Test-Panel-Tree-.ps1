<#
.Synopsis
	Test panel mixed items on TreePanel

.Description
	Points of interest:
	- root items of different types: Services, Processes, Provider items
	- root items can be optionally pre-expanded: $p.AddRoot($r, $true)
	- nested levels of different types: Providers\Drives\Items\..
	- unlimited recursive expansion: $t.Fill = $Explorer.Fill
	- items are not expandable is .Fill is not set
	- using .Data for later use of stored objects
	- using .Description for extras, use [Ctrl1]
	- using custom handler of <Enter> event
#>

$Explorer = [PowerShellFar.TreeExplorer]::new()

### Add a root for services (not yet expanded)
$r = $Explorer.RootFiles.Add()
$r.Description = 'Services and their statuses'
$r.Name = 'Services'
$r.Fill = {
	param($Explorer)
	foreach($service in (Get-Service)) {
		$t = $Explorer.ChildFiles.Add()
		$t.Data = $service
		$t.Name = $service.ServiceName
		$t.Description = $service.Status
	}
}

### Add a root for processes (not yet expanded)
$r = $Explorer.RootFiles.Add()
$r.Description = 'Processes'
$r.Name = 'Processes'
$r.Fill = {
	param($Explorer)
	foreach($process in (Get-Process)) {
		$t = $Explorer.ChildFiles.Add()
		$t.Data = $process
		$t.Name = $process.ProcessName
	}
}

### Add a root for Providers\Drives\Items\.. (pre-expanded)
$r = $Explorer.RootFiles.Add()
$r.Description = 'Providers\Drives\Items\..'
$r.Name = 'Providers'
$r.Fill = {
	param($Explorer)
	foreach($provider in (Get-PSProvider)) {
		$t = $Explorer.ChildFiles.Add()
		$t.Data = $provider
		$t.Name = $provider.Name
		$t.Description = $provider.Drives
		$t.Fill = {
			param($Explorer)
			foreach($drive in $Explorer.Data.Drives) {
				$t = $Explorer.ChildFiles.Add()
				$t.Data = $drive.Name + ':\'
				$t.Name = $drive.Name + ':'
				$t.Fill = {
					param($Explorer)
					foreach($item in (Get-ChildItem -LiteralPath $Explorer.Data -ea 0)) {
						$t = $Explorer.ChildFiles.Add()
						$t.Data = $item.PSPath
						$t.Description = $item.Description
						if ($item.PSChildName) {
							$t.Name = $item.PSChildName
						}
						else {
							$t.Name = $item.Name
						}
						if ($item.PSIsContainer) {
							$t.Fill = $Explorer.Fill
						}
					}
				}
			}
		}
	}
}
$r.Expand()

### TreePanel
$Panel = [PowerShellFar.TreePanel]::new($Explorer)
$Panel.Title = 'Test-Panel-Tree-.ps1'
$Panel.ViewMode = 1

# On <Enter>: open a child member panel for a current object
# This is the same as [CtrlPgDn], we do it for testing only.
$Panel.AsOpenFile = {
	param($Explorer, $_)
	$_.File | Open-FarPanel -Title "$($_.File.Name) opened by [Enter]" -AsChild
}

### Go
$Panel.Open()
