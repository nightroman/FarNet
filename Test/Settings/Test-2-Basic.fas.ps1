<#
.Synopsis
	ModuleSettings FarNet.Demo.Settings
#>

Add-Type -Path "$env:FARHOME\FarNet\Modules\FarNet.Demo\FarNet.Demo.dll"

job {
	# get settings instance and file name
	$settings = [FarNet.Demo.Settings]::Default
	$file = $settings.FileName

	# reset data and remove the file
	$settings.Reset()
	[IO.File]::Delete($file)

	# get data
	$data = $settings.GetData()
	Assert-Far @(
		$data.Age -eq 42
		$data.Memo.Value.Length -eq 0
		!([IO.File]::Exists($file))
	)

	# change and save
	$data.Age = 33
	$data.Paths = @($data.Paths; 'c:\temp')
	$settings.Save()

	# test file
	$lines = [IO.File]::ReadAllLines($file)
	Assert-Far @(
		$lines[4] -eq '  <Age>33</Age>'
		$lines[5] -eq '  <Memo><![CDATA[]]></Memo>'
		$lines[6] -eq '  <Regex><![CDATA[([<>&]+)]]></Regex>'
		$lines[8] -eq '    <string>%FARHOME%</string>'
		$lines[9] -eq '    <string>c:\temp</string>'
	)

	# XmlCData accepts just text
	$lines[4] = '  <Memo>bar</Memo>'
	[IO.File]::WriteAllLines($file, $lines)
	$settings.Reset()
	$data = $settings.GetData()
	Assert-Far $data.Memo.Value -eq 'bar'

	# XmlCData accepts empty element
	$lines[4] = '  <Memo/>'
	[IO.File]::WriteAllLines($file, $lines)
	$settings.Reset()
	$data = $settings.GetData()
	Assert-Far $data.Memo.Value -eq ''
}
run {
	[FarNet.Demo.Settings]::Default.Edit()
}
job {
	# diff detected
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'FarNet.Demo.Settings+Data'
}
keys Esc
job {
	# text not changed
	Assert-Far -Editor
	Assert-Far $Far.Editor[4].Text -eq '  <Memo/>'
}
keys Esc
job {
	Assert-Far -Panels
}
run {
	[FarNet.Demo.Settings]::Default.Edit()
}
keys Enter
job {
	# text changed
	Assert-Far -Editor
	Assert-Far @(
		$Far.Editor[4].Text -eq '  <Age>42</Age>'
		$Far.Editor[5].Text -eq '  <Memo><![CDATA[]]></Memo>'
		$Far.Editor[8].Text -eq '    <string>%FARHOME%</string>'
		$Far.Editor[9].Text -eq '    <string>c:\temp</string>'
	)
}
keys Esc
job {
	# save confirmation dialog
	Assert-Far -DialogTypeId f776fec0-50f7-4e7e-bda6-2a63f84a957b
}
keys Enter
