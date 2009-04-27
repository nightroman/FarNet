/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class RawUI : IRawUI
{
public:
	virtual void FlushInputBuffer();
	virtual KeyInfo ReadKey(ReadKeyOptions options);
	virtual array<BufferCell,2>^ GetBufferContents(Place rectangle);
	virtual void ScrollBufferContents(Place source, Point destination, Place clip, BufferCell fill);
	virtual void SetBufferContents(Point origin, array<BufferCell,2>^ contents);
	virtual void SetBufferContents(Place rectangle, BufferCell fill);
};
}
