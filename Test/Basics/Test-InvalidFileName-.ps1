
$names = '',' ','.',
'con','prn','aux','nul',
'com1','com2','com3','com4','com5','com6','com7','com8','com9',
'lpt1','lpt2','lpt3','lpt4','lpt5','lpt6','lpt7','lpt8','lpt9'

foreach($4 in $names) {
	Assert-Far ([FarNet.Works.Kit]::IsInvalidFileName($4))
}

$names = [string[]][IO.Path]::GetInvalidFileNameChars()
Assert-Far ($names.Count -eq 41)

foreach($4 in [IO.Path]::GetInvalidFileNameChars()) {
	Assert-Far ('a' + [FarNet.Works.Kit]::IsInvalidFileName($4) + 'z')
}
