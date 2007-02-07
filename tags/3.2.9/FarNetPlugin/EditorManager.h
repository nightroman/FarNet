#pragma once

namespace FarManagerImpl
{;
ref class Editor;

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
	void Wait(Editor^ editor);
private:
	static int CurrentEditorId();
	Editor^ GetEditorById(int id);
	Editor^ CreateEditorById(int id);
	void ToKey(const INPUT_RECORD* rec);
	void ToMouse(const INPUT_RECORD* rec);
private:
	BaseEditor^ _anyEditor;
	Dictionary<int, IEditor^>^ _editors;
	Key^ _key;
	Mouse^ _mouse;
	Editor^ _waiting;
	bool _version_1_71_2169;
};
}
