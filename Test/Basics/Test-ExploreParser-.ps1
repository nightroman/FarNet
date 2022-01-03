
Add-Type -Path $env:FARHOME\FarNet\Modules\Explore\Explore.dll

Assert-Far('1 2' -eq ([FarNet.Explore.Parser]::Tokenize('"1 2"') -join '|'))
Assert-Far('-p1|-p2|v1|-p3|1   2' -eq ([FarNet.Explore.Parser]::Tokenize('   -p1   -p2   v1   -p3   "1   2"  ') -join '|'))
Assert-Far('-p1|-p2|v1   -p3   "1   2"' -eq ([FarNet.Explore.Parser]::Tokenize('   -p1   -p2   v1   -p3   "1   2"  ', '-P2') -join '|'))

$names = @(
	"-Asynchronous"
	"-Depth"
	"-Directory"
	"-Recurse"
	"-XFile"
	"-XPath"
)

Assert-Far ([FarNet.Explore.Parser]::ResolveName('-as', $names) -eq "-Asynchronous")
Assert-Far ([FarNet.Explore.Parser]::ResolveName('-de', $names) -eq "-Depth")
Assert-Far ([FarNet.Explore.Parser]::ResolveName('-di', $names) -eq "-Directory")
Assert-Far ([FarNet.Explore.Parser]::ResolveName('-re', $names) -eq "-Recurse")
Assert-Far ([FarNet.Explore.Parser]::ResolveName('-xf', $names) -eq "-XFile")
Assert-Far ([FarNet.Explore.Parser]::ResolveName('-xp', $names) -eq "-XPath")

$err = $null
try { [FarNet.Explore.Parser]::ResolveName('-x', $names) }
catch { $err = $_; $Error.RemoveAt(0) }
Assert-Far ('Exception calling "ResolveName" with "2" argument(s): "Cannot resolve the name: -x"' -eq $err)
