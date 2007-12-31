/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#pragma once
#include "Editor.h"

namespace FarNet
{;
ref class EditorManager
{
internal:
	EditorManager();
	property BaseEditor^ AnyEditor { BaseEditor^ get() { return %_anyEditor; } }
	array<IEditor^>^ Editors();
	Editor^ GetCurrentEditor();
	int AsProcessEditorEvent(int type, void* param);
	int AsProcessEditorInput(const INPUT_RECORD* rec);
	void SetWaitingEditor(Editor^ editor);
private:
	Editor^ CreateEditorById(int id);
	Editor^ GetOrCreateEditorById(int id);
private:
	// Any editor object
	BaseEditor _anyEditor;
	// Registered opened editors
	Dictionary<int, Editor^> _editors;
	// Cached current editor
	Editor^ _editorCurrent;
	// Editor waiting for ID
	Editor^ _editorWaiting;
	// Versions
	bool _version_1_71_2169;
};
}
