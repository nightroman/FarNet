/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#include "Editor.h"

namespace FarNet
{;
ref class Editor0
{
	Editor0() {}
internal:
	static array<IEditor^>^ Editors();
	static Editor^ GetCurrentEditor();
	static int AsProcessEditorEvent(int type, void* param);
	static int AsProcessEditorInput(const INPUT_RECORD* rec);
internal:
	// Editor waiting for ID
	static Editor^ _editorWaiting;
	// Any editor object
	static AnyEditor _anyEditor;
private:
	// Registered opened editors
	static SortedList<int, Editor^> _editors;
	// Cached current editor
	static Editor^ _editorCurrent;
};
}
