#pragma once

namespace FarManagerImpl
{;
ref class VisibleEditorLineSelection;

public ref class VisibleEditorLine : public ILine
{
public:
	virtual property ILineSelection^ Selection { ILineSelection^ get(); }
	virtual property int No { int get(); }
	virtual property String^ Eol { String^ get(); void set(String^ value); }
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual String^ ToString() override;
internal:
	VisibleEditorLine(int no, bool _selected);
private:
	EditorSetString GetEss();
private:
	// Line number
	int _no;
	// Line is from selection
	bool _selected;
	// Selection; created on request
	VisibleEditorLineSelection^ _selection;
};
}
