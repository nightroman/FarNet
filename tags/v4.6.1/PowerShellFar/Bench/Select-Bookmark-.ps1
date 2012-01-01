
<#
.Synopsis
	Shows current editor bookmarks and goes to a selected one
	Author: Roman Kuzmin

.Description
	Shows a menu of bookmarks for the current file in editor and moves cursor
	to a selected one: this restores not only cursor position but the entire
	saved text frame. So called temporary stack bookmark are included, too.
#>

$Editor = $Psf.Editor()
$index = -1
$added = 0
New-FarMenu -Show 'Bookmarks' $(

	foreach($bookmark in $Editor.Bookmark.Bookmarks()) {
		++$index
		# ignore the top line
		if ($bookmark.CaretLine -ge 1 -and $bookmark.CaretLine -lt $Editor.Count) {
			New-FarItem ('&{0} {1} {2}' -f $index, (1 + $bookmark.CaretLine), $Editor[$bookmark.CaretLine]) -Data $bookmark { $Editor.Frame = $this.Data }
			++$added
		}
	}

	$bookmarks = $Editor.Bookmark.StackBookmarks()
	if ($bookmarks) {
		if ($added -gt 0) {
			New-FarItem -IsSeparator "Stack"
		}
		foreach($bookmark in $bookmarks) {
			New-FarItem ('&{0} {1}' -f (1 + $bookmark.CaretLine), $Editor[$bookmark.CaretLine]) -Data $bookmark { $Editor.Frame = $this.Data }
		}
	}
)
