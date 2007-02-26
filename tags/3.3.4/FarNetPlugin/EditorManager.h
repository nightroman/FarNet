#pragma once
#include "Editor.h"

namespace FarManagerImpl
{;
public ref class EditorManager
{
internal:
	EditorManager();
	property IAnyEditor^ AnyEditor { IAnyEditor^ get(); }
	property ICollection<IEditor^>^ Editors { ICollection<IEditor^>^ get(); }
	Editor^ CreateEditor();
	Editor^ GetCurrentEditor();
	int ProcessEditorEvent(int type, void* param);
	int ProcessEditorInput(const INPUT_RECORD* rec);
	void SetWaitingEditor(Editor^ editor);
private:
	Editor^ CreateEditorById(int id);
	Editor^ GetOrCreateEditorById(int id);
private:
	// Any editor object
	BaseEditor _anyEditor;
	// Registered opened editors
	Dictionary<int, IEditor^> _editors;
	// Cached current editor
	Editor^ _editorCurrent;
	// Editor waiting for ID
	Editor^ _editorWaiting;
	// Versions
	bool _version_1_71_2169;
};
}
