/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
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
		ebm.Cursor = new long[ei.BookMarkCount];
		ebm.LeftPos = new long[ei.BookMarkCount];
		ebm.Line = new long[ei.BookMarkCount];
		ebm.ScreenLine = new long[ei.BookMarkCount];
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

ICollection<TextFrame>^ EditorBookmark::StackBookmarks()
{
	List<TextFrame>^ r = gcnew List<TextFrame>();
	
	int count = Info.EditorControl(ECTL_GETSTACKBOOKMARKS, NULL);
	if (count <= 0)
		return r;

	EditorBookMarks ebm;
	ebm.Cursor = new long[count];
	ebm.LeftPos = new long[count];
	ebm.Line = new long[count];
	ebm.ScreenLine = new long[count];
	try
	{
		if (!Info.EditorControl(ECTL_GETSTACKBOOKMARKS, &ebm))
			throw gcnew InvalidOperationException("ECTL_GETSTACKBOOKMARKS");

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

void EditorBookmark::AddStackBookmark()
{
	if (!Info.EditorControl(ECTL_ADDSTACKBOOKMARK, NULL))
		throw gcnew InvalidOperationException("ECTL_ADDSTACKBOOKMARK");
}

void EditorBookmark::ClearStackBookmarks()
{
	if (!Info.EditorControl(ECTL_CLEARSTACKBOOKMARKS, NULL))
		throw gcnew InvalidOperationException("ECTL_CLEARSTACKBOOKMARKS");
}

void EditorBookmark::RemoveStackBookmarkAt(int index)
{
	if (!Info.EditorControl(ECTL_DELETESTACKBOOKMARK, (void*)index))
		throw gcnew InvalidOperationException("ECTL_DELETESTACKBOOKMARK");
}

//! Ignore errors
void EditorBookmark::GoToNextStackBookmark()
{
	Info.EditorControl(ECTL_NEXTSTACKBOOKMARK, NULL);
}

//! Ignore errors
void EditorBookmark::GoToPreviousStackBookmark()
{
	Info.EditorControl(ECTL_PREVSTACKBOOKMARK, NULL);
}

}
