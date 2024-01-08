
#! use a folder with 2+ subfolders, fixed issue
$root = "$env:FarNetCode\JavaScriptFar"

# get all items and depth 0, 1, 2
$all = Get-ChildItem -LiteralPath $root -Force -Recurse -Name
$0 = $all.Where{!$_.Contains('\')}
$1 = $all.Where{$_.Split('\').Length -eq 2}
$2 = $all.Where{$_.Split('\').Length -eq 3}

$explorer = [FarNet.Tools.FileSystemExplorer]::new($root)
$search = [FarNet.Tools.SearchFileCommand]::new($explorer)

task default {
	$r = @($search.Invoke())
	equals $r.Count $all.Count
}

task depth_first_search {
	$search.Bfs = $false

	$search.Depth = -1
	$r = @($search.Invoke())
	equals $r.Count $all.Count

	$search.Depth = 0
	$r = @($search.Invoke())
	equals $r.Count $0.Count

	$search.Depth = 1
	$r = @($search.Invoke())
	equals $r.Count ($0.Count + $1.Count)

	$search.Depth = 2
	$r = @($search.Invoke())
	equals $r.Count ($0.Count + $1.Count + $2.Count)
}

task breadth_first_search {
	$search.Bfs = $true

	$search.Depth = -1
	$r = @($search.Invoke())
	equals $r.Count $all.Count

	$search.Depth = 0
	$r = @($search.Invoke())
	equals $r.Count $0.Count

	$search.Depth = 1
	$r = @($search.Invoke())
	equals $r.Count ($0.Count + $1.Count)

	$search.Depth = 2
	$r = @($search.Invoke())
	equals $r.Count ($0.Count + $1.Count + $2.Count)
}

task XPath {
	$search.XPath = '//*'

	$search.Depth = -1
	$r = @($search.Invoke())
	equals $r.Count $all.Count

	$search.Depth = 0
	$r = @($search.Invoke())
	equals $r.Count $0.Count

	$search.Depth = 1
	$r = @($search.Invoke())
	equals $r.Count ($0.Count + $1.Count)

	$search.Depth = 2
	$r = @($search.Invoke())
	equals $r.Count ($0.Count + $1.Count + $2.Count)
}
