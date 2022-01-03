
Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

$explorer = [PowerShellFar.ItemExplorer]"C:\Program Files"
$search = [FarNet.Tools.SearchFileCommand]$explorer
$search.XFile = "$env:FarNetCode\Test\XPath\x-profile-2.xq"

$sw = [Diagnostics.Stopwatch]::StartNew()
$res = @($search.Invoke())
$res.Count
$sw.Elapsed.ToString()
