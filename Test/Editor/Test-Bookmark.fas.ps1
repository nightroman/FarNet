
job {
	$editor = $Far.CreateEditor()
	$editor.FileName = [Guid]::NewGuid()
	$editor.DisableHistory = $true
	$editor.Open()
	$editor.SetText(@"
0
1 bookmark 0
2
3 stack 0
4
5 stack 1
6
7 stack 2
8
`t9 bookmark 1
"@
	)
	$editor.Redraw()
}

### standard bookmarks

job {
	# end of file!
	$Frame = $Far.Editor.Frame
	Assert-Far @(
		$Frame.CaretLine -eq 9
		$Frame.CaretColumn -eq 13
		$Frame.CaretScreenColumn -eq 16
	)
}

macro 'Keys"RCtrl1 CtrlHome" -- set bookmark 1 and go home'

job {
	$Caret = $Far.Editor.Caret
	$Bookmarks = $Far.Editor.Bookmark.Bookmarks()
	Assert-Far @(
		# home!
		$Caret.Y -eq 0
		$Caret.X -eq 0
		# 10 bookmark slots
		$Bookmarks.Count -eq 10
		# fake bookmark 0!
		$Bookmarks[0].CaretLine -eq -1
		# real bookmark 1!
		$Bookmarks[1].CaretLine -eq 9
	)
}

# go to end of next, set bookmark 0
macro 'Keys"Down End RCtrl0"'

job {
	# select bookmark 1
	$Far.PostMacro('Keys("1")')
	Select-Bookmark-.ps1
}

job {
	$Caret = $Far.Editor.Caret
	$Bookmarks = $Far.Editor.Bookmark.Bookmarks()
	Assert-Far @(
		# at bookmark 1!
		$Caret.Y -eq 9
		$Caret.X -eq 13
	)
}

job {
	# select bookmark 0
	$Far.PostMacro('Keys("0")')
	Select-Bookmark-.ps1
}

job {
	$Caret = $Far.Editor.Caret
	$Bookmarks = $Far.Editor.Bookmark.Bookmarks()
	Assert-Far @(
		# at bookmark 0!
		$Caret.Y -eq 1
		$Caret.X -eq 12
	)
}

### stack bookmarks

job {
	# go to 3 lines, add stack bookmarks
	$Far.Editor.GoToLine(3)
	$Far.Editor.Bookmark.AddSessionBookmark()
	$Far.Editor.GoToLine(5)
	$Far.Editor.Bookmark.AddSessionBookmark()
	$Far.Editor.GoToLine(7)
	$Far.Editor.Bookmark.AddSessionBookmark()
	$Bookmarks = $Far.Editor.Bookmark.SessionBookmarks()
	Assert-Far @(
		$Bookmarks.Count -eq 3
		$Bookmarks[0].CaretLine -eq 3
		$Bookmarks[1].CaretLine -eq 5
		$Bookmarks[2].CaretLine -eq 7
	)
}

# go to previous by macro
# _101112_125026 Far issue corrected:
# bug? it adds one more the same bookmark, see the next block
# but: if it is done in UI via macros then it seems to work fine
macro 'BM.Prev()'

job {
	$Bookmarks = $Far.Editor.Bookmark.SessionBookmarks()
	Assert-Far @(
		# was the same line
		$Far.Editor.Caret.Y -eq 5
		# was one more same bookmark
		$Bookmarks.Count -eq 3
	)
}

# go to next by macro
macro 'BM.Next()'

job {
	Assert-Far $Far.Editor.Caret.Y -eq 7
}

job {
	# go to previous by API
	$Far.Editor.Bookmark.GoToPreviousSessionBookmark()
}
job {
	Assert-Far $Far.Editor.Caret.Y -eq 5
}

job {
	# go to next by API
	$Far.Editor.Bookmark.GoToNextSessionBookmark()
}
job {
	Assert-Far $Far.Editor.Caret.Y -eq 7
}

job {
	# remove current
	$Far.Editor.Bookmark.RemoveSessionBookmarkAt(-1)
	$Bookmarks = $Far.Editor.Bookmark.SessionBookmarks()
	Assert-Far @(
		$Bookmarks.Count -eq 2
		$Bookmarks[0].CaretLine -eq 3
		$Bookmarks[1].CaretLine -eq 5
	)
}

job {
	# remove at 1
	$Far.Editor.Bookmark.RemoveSessionBookmarkAt(1)
	$Bookmarks = $Far.Editor.Bookmark.SessionBookmarks()
	Assert-Far @(
		$Bookmarks.Count -eq 1
		$Bookmarks[0].CaretLine -eq 3
	)
}

job {
	# select bookmark 'line #4'
	$Far.PostMacro('Keys("4")')
	Select-Bookmark-.ps1
}
job {
	Assert-Far $Far.Editor.Caret.Y -eq 3
}

job {
	# clear stack bookmarks
	$Far.Editor.Bookmark.ClearSessionBookmarks()
	Assert-Far $Far.Editor.Bookmark.SessionBookmarks().Count -eq 0
}

# exit
macro 'Keys"Esc n"'
