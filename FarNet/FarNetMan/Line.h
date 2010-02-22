/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Line abstract : ILine
{
public:
	virtual property FarNet::WindowKind WindowKind { FarNet::WindowKind get() = 0; }
	virtual property ILine^ FullLine { ILine^ get() = 0; }
	virtual property ILineSelection^ Selection { ILineSelection^ get() = 0; }
	virtual property int Length { int get() = 0; }
	virtual property int No { int get(); }
	virtual property int Pos { int get() = 0; void set(int value) = 0; }
	virtual property String^ EndOfLine { String^ get(); void set(String^ value); }
	virtual property String^ Text { String^ get() = 0; void set(String^ value) = 0; }
public:
	virtual void Insert(String^ text) = 0;
	virtual void Select(int start, int end) = 0;
	virtual void Unselect() = 0;
};
}
