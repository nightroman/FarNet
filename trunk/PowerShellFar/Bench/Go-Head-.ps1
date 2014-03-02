
<#
.Synopsis
	Goes to the first panel file.
	Author: Roman Kuzmin

.Description
	The script sets current panel position to the head of files.
#>

$panel = $Far.Panel
$items = $panel.ShownList
for($i = 0; $i -lt $items.Count; ++$i) {
	$item = $items[$i]
	if ($item -and !$item.IsDirectory) {
		$panel.Redraw($i, $true)
		return
	}
}
