<#
.Synopsis
	ModuleSettings FarNet.Demo.Settings
#>

Add-Type -Path "$env:FARHOME\FarNet\Modules\FarNet.Demo\FarNet.Demo.dll"

job {
	# missing version
	$settings = [FarNet.Demo.Settings]::Default
	[IO.File]::WriteAllText($settings.FileName, @'
<?xml version="1.0"?>
<Data xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Age>33</Age>
</Data>
'@)
	$settings.Reset()
	$data = $settings.GetData()
	Assert-Far @(
		$data.Version -eq 1
		$data.Age -eq 33
		$data.Name = "Updated from version ''"
	)

	# older version
	$settings = [FarNet.Demo.Settings]::Default
	[IO.File]::WriteAllText($settings.FileName, @'
<?xml version="1.0"?>
<Data xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Version>0</Version>
  <Age>33</Age>
</Data>
'@)
	$settings.Reset()
	$data = $settings.GetData()
	Assert-Far @(
		$data.Version -eq 1
		$data.Age -eq 33
		$data.Name = "Updated from version '0'"
	)
}
