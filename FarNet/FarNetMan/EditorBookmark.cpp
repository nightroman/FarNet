
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "EditorBookmark.h"
#include "Wrappers.h"

namespace FarNet
{;
TextFrame EditorBookmark::NewTextFrame(const EditorBookMarks& bookmarks, int index)
{
	TextFrame r;
	r.CaretLine = bookmarks.Line[index];
	r.CaretColumn = bookmarks.Cursor[index];
	r.CaretScreenColumn = -1;
	r.VisibleLine = r.CaretLine - bookmarks.ScreenLine[index];
	r.VisibleChar = bookmarks.LeftPos[index];
	return r;
}

ICollection<TextFrame>^ EditorBookmark::Bookmarks()
{
	AutoEditorInfo ei;

	List<TextFrame>^ r = gcnew List<TextFrame>();
	if (ei.BookMarkCount > 0)
	{
		EditorBookMarks ebm;
		ebm.Cursor = new int[ei.BookMarkCount];
		ebm.LeftPos = new int[ei.BookMarkCount];
		ebm.Line = new int[ei.BookMarkCount];
		ebm.ScreenLine = new int[ei.BookMarkCount];
		try
		{
			EditorControl_ECTL_GETBOOKMARKS(ebm);

			r->Capacity = ei.BookMarkCount;
			for(int i = 0; i < ei.BookMarkCount; ++i)
				r->Add(NewTextFrame(ebm, i));
		}
		finally
		{
			delete ebm.Cursor;
			delete ebm.LeftPos;
			delete ebm.Line;
			delete ebm.ScreenLine;
		}
	}

	return r;
}

ICollection<TextFrame>^ EditorBookmark::SessionBookmarks()
{
	List<TextFrame>^ r = gcnew List<TextFrame>();

	int count = (int)Info.EditorControl(-1, ECTL_GETSESSIONBOOKMARKS, 0, 0);
	if (count <= 0)
		return r;

	EditorBookMarks ebm;
	ebm.Cursor = new int[count];
	ebm.LeftPos = new int[count];
	ebm.Line = new int[count];
	ebm.ScreenLine = new int[count];
	try
	{
		if (!Info.EditorControl(-1, ECTL_GETSESSIONBOOKMARKS, 0, &ebm))
			throw gcnew InvalidOperationException("ECTL_GETSESSIONBOOKMARKS");

		r->Capacity = count;
		for(int i = 0; i < count; ++i)
			r->Add(NewTextFrame(ebm, i));
	}
	finally
	{
		delete ebm.Cursor;
		delete ebm.LeftPos;
		delete ebm.Line;
		delete ebm.ScreenLine;
	}

	return r;
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
