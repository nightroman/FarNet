
<#
.SYNOPSIS
	Shows current editor bookmarks and goes to a selected
	Author: Roman Kuzmin

.DESCRIPTION
	Shows a menu of bookmarks for the current file in editor and moves cursor
	to a selected one: this restores not only cursor position but the entire
	saved text frame, i.e. top line and left position, too.
#>

$Editor = $Psf.Editor()
$i = 0
New-FarMenu -Show 'Bookmarks' $(
	foreach($b in $Editor.Bookmarks()) {
		if ($b.Line -ge 1 -and $b.Line -lt $Editor.Count) {
			New-FarItem ("&{0} {1}" -f $i, $Editor[$b.Line]) -Data $b { $Editor.Frame = $this.Data }
		}
		++$i
	}
)
