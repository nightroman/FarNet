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
	virtual property String^ ConsoleTitle { String^ get() override; }
	virtual property Object^ Shelve { Object^ get() override; }
public:
	virtual array<Works::BufferCell, 2>^ GetBufferContents(Place rectangle) override;
	virtual KeyInfo ReadKey(Works::ReadKeyOptions options) override;
	virtual void FlushInputBuffer() override;
	virtual void ScrollBufferContents(Place source, Point destination, Place clip, Works::BufferCell fill) override;
	virtual void SetBufferContents(Place rectangle, Works::BufferCell fill) override;
	virtual void SetBufferContents(Point origin, array<Works::BufferCell, 2>^ contents) override;
	virtual void Break() override;
	virtual MacroParseError^ CheckMacro(String^ sequence, bool silent) override;
	virtual void LoadMacros() override;
	virtual void SaveMacros() override;
};
}
