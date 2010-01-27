@{
	GUID = '{c1769b3b-e066-4042-bbea-be7a599562d3}'
	Author = 'Roman Kuzmin'
	CompanyName = 'http://code.google.com/p/farnet/'
	Copyright = '© 2010 Roman Kuzmin. All rights reserved.'
	Description = 'Far Manager macro provider and tools.'
	ModuleVersion = '1.0.0'

	NestedModules = 'FarMacro.dll'
    #ModuleToProcess = 'Pscx.psm1'
	FormatsToProcess = @('FarMacro.Format.ps1xml')

	CmdletsToExport = '*'

	CLRVersion = '2.0.50727'
	PowerShellVersion = '2.0'
	PowerShellHostName = 'FarHost'
	PowerShellHostVersion = '4.3.1'
}
