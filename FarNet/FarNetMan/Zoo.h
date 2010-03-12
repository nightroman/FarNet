/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Zoo : Works::IZoo
{
public:
	virtual property String^ ConsoleTitle { String^ get(); }
	virtual property Object^ Shelve { Object^ get(); }
public:
	virtual array<Works::BufferCell, 2>^ GetBufferContents(Place rectangle);
	virtual KeyInfo ReadKey(Works::ReadKeyOptions options);
	virtual void FlushInputBuffer();
	virtual void ScrollBufferContents(Place source, Point destination, Place clip, Works::BufferCell fill);
	virtual void SetBufferContents(Place rectangle, Works::BufferCell fill);
	virtual void SetBufferContents(Point origin, array<Works::BufferCell, 2>^ contents);
	virtual void Break();
};
}
