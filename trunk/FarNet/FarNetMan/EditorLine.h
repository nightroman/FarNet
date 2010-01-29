/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#include "Line.h"

namespace FarNet
{;
ref class EditorLineSelection;

ref class EditorLine sealed : Line
{
public:
	virtual property FarNet::WindowType WindowType { FarNet::WindowType get() override; }
	virtual property ILine^ FullLine { ILine^ get() override; }
	virtual property ILineSelection^ Selection { ILineSelection^ get() override; }
	virtual property int Length { int get() override; }
	virtual property int No { int get() override; }
	virtual property int Pos { int get() override; void set(int value) override; }
	virtual property String^ EndOfLine { String^ get() override; void set(String^ value) override; }
	virtual property String^ Text { String^ get() override; void set(String^ value) override; }
public:
	virtual void Insert(String^ text) override;
	virtual void Select(int start, int end) override;
	virtual void Unselect() override;
public:
	virtual String^ ToString() override;
internal:
	EditorLine(int no, bool selected);
private:
	EditorSetString GetEss();
private:
	// Line number
	int _no;
	// Line is from selection
	bool _selected;
	// Selection; created on request
	EditorLineSelection^ _selection;
};
}
