<#
.Synopsis
	Shows editor bookmarks and navigates to selected.
	Author: Roman Kuzmin

.Description
	Call it from the current editor to show saved and session bookmarks,
	navigate to selected.
#>

#requires -Version 7.4
$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

$Editor = $Psf.Editor()
$index = -1
$added = 0

$click = {
	$Editor.Frame = $_.Item.Data
	$Editor.Redraw()
}

New-FarMenu -Show Bookmarks @(
	foreach($bookmark in $Editor.Bookmark.Bookmarks()) {
		++$index
		# ignore the top line
		if ($bookmark.CaretLine -ge 1 -and $bookmark.CaretLine -lt $Editor.Count) {
			New-FarItem ('&{0} {1} {2}' -f $index, (1 + $bookmark.CaretLine), $Editor[$bookmark.CaretLine]) $click -Data $bookmark
			++$added
		}
	}

	$bookmarks = $Editor.Bookmark.SessionBookmarks()
	if ($bookmarks) {
		if ($added -gt 0) {
			New-FarItem Session -IsSeparator
		}
		foreach($bookmark in $bookmarks) {
			New-FarItem ('&{0} {1}' -f (1 + $bookmark.CaretLine), $Editor[$bookmark.CaretLine]) $click -Data $bookmark
		}
	}
)
