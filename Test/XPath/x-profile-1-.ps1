
$explorer = [PowerShellFar.ItemExplorer] "C:\Program Files" #'C:\', "C:\Program Files"
$search = [FarNet.Tools.SearchFileCommand]$explorer
$search.XFile = "$env:FarNetCode\Test\XPath\x-profile-1.xq"

$sw = [Diagnostics.Stopwatch]::StartNew()
$res = @($search.Invoke())
$res.Count
$sw.Elapsed.ToString()
