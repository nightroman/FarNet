
#! use a folder with 2+ subfolders, to cover a fixed issue
$root = "$env:FarNetCode\JavaScriptFar"

# get all items and depth 0, 1, 2
$all = Get-ChildItem -LiteralPath $root -Force -Recurse -Name
$0 = $all.Where{!$_.Contains('\')}
$1 = $all.Where{$_.Split('\').Length -eq 2}
$2 = $all.Where{$_.Split('\').Length -eq 3}

$explorer = [FarNet.Tools.FileSystemExplorer]::new($root)
$search = [FarNet.Tools.SearchFileCommand]::new($explorer)

### default
$r = @($search.Invoke())
Assert-Far $r.Count -eq $all.Count

### depth-first-search
$search.Bfs = $false

$search.Depth = -1
$r = @($search.Invoke())
Assert-Far $r.Count -eq $all.Count

$search.Depth = 0
$r = @($search.Invoke())
Assert-Far $r.Count -eq $0.Count

$search.Depth = 1
$r = @($search.Invoke())
Assert-Far $r.Count -eq ($0.Count + $1.Count)

$search.Depth = 2
$r = @($search.Invoke())
Assert-Far $r.Count -eq ($0.Count + $1.Count + $2.Count)

### Breadth
$search.Bfs = $true

$search.Depth = -1
$r = @($search.Invoke())
Assert-Far $r.Count -eq $all.Count

$search.Depth = 0
$r = @($search.Invoke())
Assert-Far $r.Count -eq $0.Count

$search.Depth = 1
$r = @($search.Invoke())
Assert-Far $r.Count -eq ($0.Count + $1.Count)

$search.Depth = 2
$r = @($search.Invoke())
Assert-Far $r.Count -eq ($0.Count + $1.Count + $2.Count)
