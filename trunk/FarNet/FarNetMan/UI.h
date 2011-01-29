/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class FarUI sealed : IUserInterface
{
public:
	virtual property bool KeyAvailable { bool get() override; }
	virtual property ConsoleColor BackgroundColor { ConsoleColor get() override; void set(ConsoleColor value) override; }
	virtual property ConsoleColor ForegroundColor { ConsoleColor get() override; void set(ConsoleColor value) override; }
	virtual property int CursorSize { int get() override; void set(int value) override; }
	virtual property IntPtr MainWindowHandle { IntPtr get() override; }
	virtual property Place WindowPlace { Place get() override; }
	virtual property Point BufferCursor { Point get() override; void set(Point value) override; }
	virtual property Point BufferSize { Point get() override; void set(Point value) override; }
	virtual property Point MaxPhysicalWindowSize { Point get() override; }
	virtual property Point MaxWindowSize { Point get() override; }
	virtual property Point WindowCursor { Point get() override; void set(Point value) override; }
	virtual property Point WindowPoint { Point get() override; void set(Point value) override; }
	virtual property Point WindowSize { Point get() override; void set(Point value) override; }
	virtual property String^ WindowTitle { String^ get() override; void set(String^ value) override; }
public:
	virtual array<Works::BufferCell, 2>^ GetBufferContents(Place rectangle) override;
	virtual ConsoleColor GetPaletteBackground(PaletteColor paletteColor) override;
	virtual ConsoleColor GetPaletteForeground(PaletteColor paletteColor) override;
	virtual int ReadKeys(array<int>^ virtualKeyCodes) override;
	virtual int SaveScreen(int x1, int y1, int x2, int y2) override;
	virtual KeyInfo ReadKey(Works::ReadKeyOptions options) override;
	virtual void Break() override;
	virtual void Clear() override;
	virtual void Draw() override;
	virtual void DrawColor(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text) override;
	virtual void DrawPalette(int left, int top, PaletteColor paletteColor, String^ text) override;
	virtual void FlushInputBuffer() override;
	virtual void Redraw() override;
	virtual void RestoreScreen(int screen) override;
	virtual void SaveUserScreen() override;
	virtual void SetProgressFlash() override;
	virtual void SetProgressState(TaskbarProgressBarState state) override;
	virtual void SetProgressValue(int currentValue, int maximumValue) override;
	virtual void ScrollBufferContents(Place source, Point destination, Place clip, Works::BufferCell fill) override;
	virtual void SetBufferContents(Place rectangle, Works::BufferCell fill) override;
	virtual void SetBufferContents(Point origin, array<Works::BufferCell, 2>^ contents) override;
	virtual void ShowUserScreen() override;
	virtual void Write(String^ text) override;
	virtual void Write(String^ text, ConsoleColor foregroundColor) override;
	virtual void Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor) override;
internal:
	static FarUI Instance;
};
}
