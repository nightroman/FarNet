/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Zoo : IZoo
{
public:
	virtual property int OemCP { int get(); }
	virtual property String^ ConsoleTitle { String^ get(); }
	virtual property Object^ Shelve { Object^ get(); }
public:
	virtual array<BufferCell,2>^ GetBufferContents(Place rectangle);
	virtual KeyInfo ReadKey(ReadKeyOptions options);
	virtual void FlushInputBuffer();
	virtual void ScrollBufferContents(Place source, Point destination, Place clip, BufferCell fill);
	virtual void SetBufferContents(Place rectangle, BufferCell fill);
	virtual void SetBufferContents(Point origin, array<BufferCell,2>^ contents);
	virtual void Break();
};
}
