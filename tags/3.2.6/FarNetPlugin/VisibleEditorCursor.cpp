#include "StdAfx.h"
#include "VisibleEditorCursor.h"
#include "Utils.h"

namespace FarManagerImpl
{;
VisibleEditorCursor::VisibleEditorCursor()
{
}

#define __get(X) EditorInfo ei; EditorControl_ECTL_GETINFO(ei); return ei.X
#define __set(X) SEditorSetPosition esp; esp.X = value; PutEsp(esp)

int VisibleEditorCursor::LeftPos::get()
{
	__get(LeftPos);
}

void VisibleEditorCursor::LeftPos::set(int value)
{
	__set(LeftPos);
}

int VisibleEditorCursor::Line::get()
{
	__get(CurLine);
}

void VisibleEditorCursor::Line::set(int value)
{
	__set(CurLine);
}

int VisibleEditorCursor::Pos::get()
{
	__get(CurPos);
}

void VisibleEditorCursor::Pos::set(int value)
{
	__set(CurPos);
}

int VisibleEditorCursor::TabPos::get()
{
	__get(CurTabPos);
}

void VisibleEditorCursor::TabPos::set(int value)
{
	__set(CurTabPos);
}

int VisibleEditorCursor::TopLine::get()
{
	__get(TopScreenLine);
}

void VisibleEditorCursor::TopLine::set(int value)
{
	__set(TopScreenLine);
}

void VisibleEditorCursor::Assign(ICursor^ cursor)
{
	if (cursor == nullptr)
		throw gcnew ArgumentNullException("cursor");
	EditorSetPosition esp;
	esp.CurLine = cursor->Line;
	esp.CurPos = cursor->Pos;
	esp.CurTabPos = cursor->TabPos;
	esp.LeftPos = cursor->LeftPos;
	esp.TopScreenLine = cursor->TopLine;
	esp.Overtype = -1;
	PutEsp(esp);
}

void VisibleEditorCursor::Set(int pos, int line)
{
	SEditorSetPosition esp;
	esp.CurPos = pos;
	esp.CurLine = line;
	PutEsp(esp);
}

void VisibleEditorCursor::PutEsp(const EditorSetPosition& esp)
{
	Info.EditorControl(ECTL_SETPOSITION, (EditorSetPosition*)&esp);
}
}
