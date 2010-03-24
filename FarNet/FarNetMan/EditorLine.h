/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class EditorLine sealed : ILine
{
public:
	virtual property FarNet::WindowKind WindowKind { FarNet::WindowKind get() override; }
	virtual property int Caret { int get() override; void set(int value) override; }
	virtual property int Index { int get() override; }
	virtual property int Length { int get() override; }
	virtual property Span Selection { Span get() override; }
	virtual property String^ Text { String^ get() override; void set(String^ value) override; }
	virtual property String^ SelectedText { String^ get() override; void set(String^ value) override; }
public:
	virtual void InsertText(String^ text) override;
	virtual void SelectText(int start, int end) override;
	virtual void UnselectText() override;
internal:
	EditorLine(int index);
	EditorLine(int index, bool selected);
private:
	EditorSetString GetEss();
private:
	const int _Index;
	bool _Selected;
};
}
