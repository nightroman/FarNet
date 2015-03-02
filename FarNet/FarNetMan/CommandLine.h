
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

#pragma once

namespace FarNet
{;
ref class CommandLine sealed : ILine
{
public:
	virtual property FarNet::WindowKind WindowKind { FarNet::WindowKind get() override; }
	virtual property int Caret { int get() override; void set(int value) override; }
	virtual property int Length { int get() override; }
	virtual property Span SelectionSpan { Span get() override; }
	virtual property String^ Text { String^ get() override; void set(String^ value) override; }
	virtual property String^ SelectedText { String^ get() override; void set(String^ value) override; }
public:
	virtual void InsertText(String^ text) override;
	virtual void SelectText(int start, int end) override;
	virtual void UnselectText() override;
};
}
