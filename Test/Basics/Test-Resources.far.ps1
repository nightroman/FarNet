<#
.Synopsis
	Tests module localized resource management.
#>

Set-StrictMode -Version 2

$manager = $Far.GetModuleManager('FarNet.Demo')
$null = $manager.LoadAssembly($true)

# current culture
$uic1 = $manager.CurrentUICulture

# test a string
$str1 = $manager.GetString('MenuTitle')
Assert-Far ($str1 -eq 'Трассировка' -or $str2 -eq 'Tracing')

# test and swap culture
if ($uic1.Name -eq "ru") {
	$manager.CurrentUICulture = [Globalization.CultureInfo]::GetCultureInfo("en")
}
elseif ($uic1.Name -eq "en") {
	$manager.CurrentUICulture = [Globalization.CultureInfo]::GetCultureInfo("ru")
}
else {
	throw 'Unexpected culture'
}

# test the same string with another culture
$str2 = $manager.GetString('MenuTitle')
Assert-Far ($str1 -ne $str2)
Assert-Far ($str1 -eq 'Трассировка' -or $str2 -eq 'Tracing')

# restore culture
$manager.CurrentUICulture = $uic1
