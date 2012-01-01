@{
	Author = 'Roman Kuzmin'
	ModuleVersion = '1.0.0'
	CompanyName = 'http://code.google.com/p/farnet/'
	Description = 'Far Manager file description tools.'
	Copyright = '(C) 2010 Roman Kuzmin. All rights reserved.'

	NestedModules = 'FarDescription.dll'
	ModuleToProcess = 'FarDescription.psm1'
	RequiredAssemblies = 'FarDescription.dll'
	TypesToProcess = @('FarDescription.Types.ps1xml')

	CLRVersion = '2.0.50727'
	PowerShellVersion = '2.0'
	GUID = '{1e7f7fc4-59c4-48c6-8847-bddef25458dd}'
}
