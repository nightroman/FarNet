#pragma once

namespace FarManagerImpl
{;
ref class VisibleEditorLineSelection;

public ref class VisibleEditorLine : public ILine
{
public:
	virtual property ILine^ FullLine { ILine^ get(); }
	virtual property ILineSelection^ Selection { ILineSelection^ get(); }
	virtual property int No { int get(); }
	virtual property int Pos { int get(); void set(int value); }
	virtual property String^ Eol { String^ get(); void set(String^ value); }
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual void Insert(String^ text);
	virtual void Select(int start, int end);
	virtual void Unselect();
public:
	virtual String^ ToString() override;
internal:
	VisibleEditorLine(int no, bool selected);
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
