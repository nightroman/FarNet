/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class CommandLine : ILine
{
public:
	virtual property ILine^ FullLine { ILine^ get(); }
	virtual property ILineSelection^ Selection { ILineSelection^ get(); }
	virtual property int Length { int get(); }
	virtual property int No { int get(); }
	virtual property int Pos { int get(); void set(int value); }
	virtual property String^ Eol { String^ get(); void set(String^ value); }
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual property FarNet::WindowType WindowType { FarNet::WindowType get() { return FarNet::WindowType::Panels; } }
public:
	virtual void Insert(String^ text);
	virtual void Select(int start, int end);
	virtual void Unselect();
public:
	virtual String^ ToString() override;
};
}
