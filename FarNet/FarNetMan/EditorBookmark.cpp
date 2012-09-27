
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "EditorBookmark.h"
#include "Wrappers.h"

namespace FarNet
{;
static TextFrame NewTextFrame(const EditorBookmarks& bookmarks, int index)
{
	TextFrame r;
	r.CaretLine = (int)bookmarks.Line[index];
	r.CaretColumn = (int)bookmarks.Cursor[index];
	r.CaretScreenColumn = -1;
	r.VisibleLine = r.CaretLine - (int)bookmarks.ScreenLine[index];
	r.VisibleChar = (int)bookmarks.LeftPos[index];
	return r;
}

static ICollection<TextFrame>^ GetBookmarks(EDITOR_CONTROL_COMMANDS command)
{
	size_t size = Info.EditorControl(-1, command, 0, 0);
	if (size == 0)
		return gcnew List<TextFrame>();
	
	char* buffer = new char[size];
	EditorBookmarks& ebm = *(EditorBookmarks*)buffer;
	ebm.StructSize = sizeof(EditorBookmarks);
	ebm.Size = size;

	Info.EditorControl(-1, command, 0, buffer);
	List<TextFrame>^ r = gcnew List<TextFrame>((int)ebm.Count);

	for(size_t i = 0; i < ebm.Count; ++i)
		r->Add(NewTextFrame(ebm, (int)i));
	
	delete buffer;
	return r;
}

ICollection<TextFrame>^ EditorBookmark::Bookmarks()
{
	return GetBookmarks(ECTL_GETBOOKMARKS);
}
ICollection<TextFrame>^ EditorBookmark::SessionBookmarks()
{
	return GetBookmarks(ECTL_GETSESSIONBOOKMARKS);
}

void EditorBookmark::AddSessionBookmark()
{
	if (!Info.EditorControl(-1, ECTL_ADDSESSIONBOOKMARK, 0, 0))
		throw gcnew InvalidOperationException("ECTL_ADDSESSIONBOOKMARK");
}

void EditorBookmark::ClearSessionBookmarks()
{
	if (!Info.EditorControl(-1, ECTL_CLEARSESSIONBOOKMARKS, 0, 0))
		throw gcnew InvalidOperationException("ECTL_CLEARSESSIONBOOKMARKS");
}

void EditorBookmark::RemoveSessionBookmarkAt(int index)
{
	if (!Info.EditorControl(-1, ECTL_DELETESESSIONBOOKMARK, 0, (void*)index))
		throw gcnew InvalidOperationException("ECTL_DELETESESSIONBOOKMARK");
}

//! Ignore errors
void EditorBookmark::GoToNextSessionBookmark()
{
	Info.EditorControl(-1, ECTL_NEXTSESSIONBOOKMARK, 0, 0);
}

//! Ignore errors
void EditorBookmark::GoToPreviousSessionBookmark()
{
	Info.EditorControl(-1, ECTL_PREVSESSIONBOOKMARK, 0, 0);
}

}
