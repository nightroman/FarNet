
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
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
	static int AsProcessEditorEvent(const ProcessEditorEventInfo* info);
	static int AsProcessEditorInput(const ProcessEditorInputInfo* info);
internal:
	// Editor waiting for ID
	static Editor^ _editorWaiting;
	// Any editor object
	static AnyEditor _anyEditor;
private:
	static void ConnectEditor(Editor^ editor, const EditorInfo& ei, bool isEditorWaiting);
	static int FindEditor(intptr_t id);
private:
	// Opened editors
	static List<Editor^> _editors;
};
}
