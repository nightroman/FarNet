
<#
.Synopsis
	Goes to the head of panel files or directories.
	Author: Roman Kuzmin

.Description
	The script sets current panel position to the head of files or to the head
	of all items depending on the current item type (file or directory) and its
	position.
#>

$panel = $Far.Panel
$item = $panel.CurrentFile
$index = $panel.CurrentIndex

if ($index -ne 0 -and $item.IsDirectory) {
	$panel.Redraw(0, $true)
	return
}

$items = $panel.ShownList
for($i = 0; $i -lt $items.Count; ++$i) {
	$item = $items[$i]
	if ($item -and !$item.IsDirectory) {
		if ($i -eq $index) {
			$panel.Redraw(0, $true)
		}
		else {
			$panel.Redraw($i, $true)
		}
		return
	}
}
