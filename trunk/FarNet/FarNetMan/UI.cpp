
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "UI.h"
#include "Wrappers.h"

// Here only we use Console
#undef Console

namespace FarNet
{;
static void ThrowWithLastError(String^ message)
{
	throw gcnew InvalidOperationException(message + " error code: " + GetLastError());
}

static FarColor GetFarPaletteColor(PaletteColor paletteColor)
{
	int index = (int)paletteColor;
	if (index < 0 || index >= Wrap::GetEndPalette())
		throw gcnew ArgumentOutOfRangeException("paletteColor");

	FarColor arg;
	Info.AdvControl(&MainGuid, ACTL_GETCOLOR, index, &arg);
	return arg;
}

int FarUI::CursorSize::get()
{
	return Console::CursorSize;
}
void FarUI::CursorSize::set(int value)
{
	Console::CursorSize = value;
}

ConsoleColor FarUI::BackgroundColor::get()
{
	return Console::BackgroundColor;
}
void FarUI::BackgroundColor::set(ConsoleColor value)
{
	Console::BackgroundColor = value;
}

ConsoleColor FarUI::ForegroundColor::get()
{
	return Console::ForegroundColor;
}
void FarUI::ForegroundColor::set(ConsoleColor value)
{
	Console::ForegroundColor = value;
}

Place FarUI::WindowPlace::get()
{
	SMALL_RECT rect;
	Info.AdvControl(&MainGuid, ACTL_GETFARRECT, 0, &rect);
	return Place(rect.Left, rect.Top, rect.Right, rect.Bottom);
}

Point FarUI::WindowPoint::get()
{
	SMALL_RECT rect;
	Info.AdvControl(&MainGuid, ACTL_GETFARRECT, 0, &rect);
	return Point(rect.Left, rect.Top);
}
void FarUI::WindowPoint::set(Point value)
{
	Console::SetWindowPosition(value.X, value.Y);
}

Point FarUI::WindowSize::get()
{
	SMALL_RECT rect;
	Info.AdvControl(&MainGuid, ACTL_GETFARRECT, 0, &rect);
	return Point(rect.Right - rect.Left + 1, rect.Bottom - rect.Top + 1);
}
void FarUI::WindowSize::set(Point value)
{
	Console::SetWindowSize(value.X, value.Y);
}

Point FarUI::BufferCursor::get()
{
	return Point(Console::CursorLeft, Console::CursorTop);
}
void FarUI::BufferCursor::set(Point value)
{
	Console::SetCursorPosition(value.X, value.Y);
}

Point FarUI::BufferSize::get()
{
	return Point(Console::BufferWidth, Console::BufferHeight);
}
void FarUI::BufferSize::set(Point value)
{
	Console::SetBufferSize(value.X, value.Y);
}

Point FarUI::WindowCursor::get()
{
	COORD pos;
	Info.AdvControl(&MainGuid, ACTL_GETCURSORPOS, 0, &pos);
	return Point(pos.X, pos.Y);
}
void FarUI::WindowCursor::set(Point value)
{
	COORD pos;
	pos.X = (SHORT)value.X;
	pos.Y = (SHORT)value.Y;
	Info.AdvControl(&MainGuid, ACTL_SETCURSORPOS, 0, &pos);
}

void FarUI::FlushInputBuffer()
{
	HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);
	if (hStdin == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	if (!FlushConsoleInputBuffer(hStdin))
		ThrowWithLastError("FlushConsoleInputBuffer");
}

bool FarUI::KeyAvailable::get()
{
	return Console::KeyAvailable;
}

KeyInfo^ FarUI::ReadKey(Works::ReadKeyOptions options)
{
	if (int(options & (Works::ReadKeyOptions::IncludeKeyDown | Works::ReadKeyOptions::IncludeKeyUp)) == 0)
		throw gcnew ArgumentException("Argument 'options': either IncludeKeyDown, IncludeKeyUp or both must be set.");

	HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);
	if (hStdin == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	DWORD mode1;
	if (!GetConsoleMode(hStdin, &mode1))
		ThrowWithLastError("GetConsoleMode");

	DWORD mode2 = 0;
	if (int(options & Works::ReadKeyOptions::NoEcho) == 0)
		mode2 |= (ENABLE_ECHO_INPUT | ENABLE_LINE_INPUT);
	if (int(options & Works::ReadKeyOptions::AllowCtrlC) != 0)
		mode2 |= ENABLE_PROCESSED_INPUT;

	try
	{
		if (!SetConsoleMode(hStdin, mode2))
			ThrowWithLastError("SetConsoleMode");

		for(;;)
		{
			INPUT_RECORD ir;
			DWORD numberOfEventsRead;
			if (!ReadConsoleInput(hStdin, &ir, 1, &numberOfEventsRead))
				ThrowWithLastError("ReadConsoleInput");
			if (ir.EventType != KEY_EVENT)
				continue;
			if (ir.Event.KeyEvent.bKeyDown)
			{
				if (int(options & Works::ReadKeyOptions::IncludeKeyDown) == 0)
					continue;
			}
			else
			{
				if (int(options & Works::ReadKeyOptions::IncludeKeyUp) == 0)
					continue;
			}
			return KeyInfoFromInputRecord(ir);
		}
	}
	finally
	{
		SetConsoleMode(hStdin, mode1);
	}
}

// _091007_034112
// Getting console Title throws an exception internally caught by PowerShell. Usually in MT scenarios.
// It does not make problems but it is noisy. So we use a native call with no exceptions.
String^ FarUI::WindowTitle::get()
{
	// .NET uses buf[0x5fb5] size
	// CA: C6262: Function uses '49008' bytes of stack: exceeds /analyze:stacksize'16384'. Consider moving some data to heap.
	wchar_t buf[4096];
	if (::GetConsoleTitle(buf, countof(buf)))
		return gcnew String(buf);

	return String::Empty;
}

void FarUI::WindowTitle::set(String^ value)
{
	Console::Title = value;
}

Point FarUI::MaxPhysicalWindowSize::get()
{
	return Point(Console::LargestWindowWidth, Console::LargestWindowHeight);
}

Point FarUI::MaxWindowSize::get()
{
	return Point(
		Math::Min(Console::LargestWindowWidth, Console::BufferWidth),
		Math::Min(Console::LargestWindowHeight, Console::BufferHeight));
}

void FarUI::ScrollBufferContents(Place source, Point destination, Place clip, Works::BufferCell fill)
{
	HANDLE hStdout = GetStdHandle(STD_OUTPUT_HANDLE);
	if (hStdout == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	SMALL_RECT source1;
	source1.Bottom = (SHORT)source.Bottom;
	source1.Left = (SHORT)source.Left;
	source1.Right = (SHORT)source.Right;
	source1.Top = (SHORT)source.Top;
	SMALL_RECT clip1;
	clip1.Bottom = (SHORT)clip.Bottom;
	clip1.Left = (SHORT)clip.Left;
	clip1.Right = (SHORT)clip.Right;
	clip1.Top = (SHORT)clip.Top;
	COORD destination1;
	destination1.X = (SHORT)destination.X;
	destination1.Y = (SHORT)destination.Y;
	CHAR_INFO fill1;
	fill1.Char.UnicodeChar = fill.Character;
	fill1.Attributes = COMMON_LVB_LEADING_BYTE | COMMON_LVB_TRAILING_BYTE | (WORD)fill.ForegroundColor | ((WORD)fill.BackgroundColor << 4);

	if (!ScrollConsoleScreenBuffer(hStdout, &source1, &clip1, destination1, &fill1))
		ThrowWithLastError("ScrollConsoleScreenBuffer");
}

array<Works::BufferCell, 2>^ FarUI::GetBufferContents(Place rectangle)
{
	HANDLE hStdout = GetStdHandle(STD_OUTPUT_HANDLE);
	if (hStdout == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	// requested size
	const int w1 = rectangle.Right - rectangle.Left + 1;
	const int h1 = rectangle.Bottom - rectangle.Top + 1;

	CHAR_INFO* buf = new CHAR_INFO[w1*h1];
	try
	{
		COORD coordBufSize;
		coordBufSize.X = (SHORT)w1;
		coordBufSize.Y = (SHORT)h1;
		COORD coordBufCoord;
		coordBufCoord.X = 0;
		coordBufCoord.Y = 0;
		SMALL_RECT srctReadRect;
		srctReadRect.Bottom = (SHORT)rectangle.Bottom;
		srctReadRect.Left = (SHORT)rectangle.Left;
		srctReadRect.Right = (SHORT)rectangle.Right;
		srctReadRect.Top = (SHORT)rectangle.Top;

		if (!ReadConsoleOutput(hStdout, buf, coordBufSize, coordBufCoord, &srctReadRect))
			ThrowWithLastError("ReadConsoleOutput");

		// actual size
		const int w2 = srctReadRect.Right - srctReadRect.Left + 1;
		const int h2 = srctReadRect.Bottom - srctReadRect.Top + 1;

		// fill result padding with an empty cell
		Works::BufferCell empty(' ', Console::ForegroundColor, Console::BackgroundColor, Works::BufferCellType::Complete);
		array<Works::BufferCell, 2>^ r = gcnew array<Works::BufferCell, 2>(h1, w1);
		for(int i = 0, k = 0; i < h2; ++i)
		{
			for(int j = 0; j < w2; ++j)
			{
				CHAR_INFO& ci = buf[k];
				r[i, j] = Works::BufferCell(ci.Char.UnicodeChar, (ConsoleColor)(ci.Attributes & 0xF), (ConsoleColor)((ci.Attributes & 0xF0) >> 4), Works::BufferCellType::Complete);
				++k;
			}
			for(int j = w2; j < w1; ++j)
			{
				r[i, j] = empty;
				++k;
			}
		}
		for(int i = h2; i < h1; ++i)
		{
			for(int j = 0; j < w1; ++j)
				r[i, j] = empty;
		}

		return r;
	}
	finally
	{
		delete[] buf;
	}
}

void FarUI::SetBufferContents(Point origin, array<Works::BufferCell, 2>^ contents)
{
	HANDLE hStdout = GetStdHandle(STD_OUTPUT_HANDLE);
	if (hStdout == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	COORD bufSize;
	bufSize.X = (SHORT)contents->GetLength(1);
	bufSize.Y = (SHORT)contents->GetLength(0);
	COORD bufOrigin;
	bufOrigin.X = 0;
	bufOrigin.Y = 0;
	SMALL_RECT rect;
	rect.Top = (SHORT)origin.Y;
	rect.Left = (SHORT)origin.X;
	rect.Right = rect.Left + bufSize.X - 1;
	rect.Bottom = rect.Top + bufSize.Y - 1;

	CHAR_INFO* buf = new CHAR_INFO[bufSize.X*bufSize.Y];
	try
	{
		for(int i = 0, k = 0; i < bufSize.Y; ++i)
		{
			for(int j = 0; j < bufSize.X; ++j)
			{
				CHAR_INFO& ci = buf[k];
				Works::BufferCell fill = contents[i, j];
				ci.Char.UnicodeChar = fill.Character;
				ci.Attributes = COMMON_LVB_LEADING_BYTE | COMMON_LVB_TRAILING_BYTE | (WORD)fill.ForegroundColor | ((WORD)fill.BackgroundColor << 4);
				++k;
			}
		}
		if (!WriteConsoleOutput(hStdout, buf, bufSize, bufOrigin, &rect))
			ThrowWithLastError("WriteConsoleOutput");
	}
	finally
	{
		delete[] buf;
	}
}

void FarUI::SetBufferContents(Place rectangle, Works::BufferCell fill)
{
	HANDLE hStdout = GetStdHandle(STD_OUTPUT_HANDLE);
	if (hStdout == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	if (rectangle.Top < 0 || rectangle.Left < 0 || rectangle.Right < 0 || rectangle.Bottom < 0)
		rectangle = Place(0, 0, Console::BufferWidth - 1, Console::BufferHeight - 1);

	COORD bufSize;
	bufSize.X = (SHORT)(rectangle.Right - rectangle.Left + 1);
	bufSize.Y = (SHORT)(rectangle.Bottom - rectangle.Top + 1);
	COORD bufOrigin;
	bufOrigin.X = 0;
	bufOrigin.Y = 0;
	SMALL_RECT rect;
	rect.Top = (SHORT)rectangle.Top;
	rect.Left = (SHORT)rectangle.Left;
	rect.Right = rect.Left + bufSize.X - 1;
	rect.Bottom = rect.Top + bufSize.Y - 1;

	int total = bufSize.X*bufSize.Y;
	CHAR_INFO* buf = new CHAR_INFO[total];
	CHAR_INFO ci;
	ci.Char.UnicodeChar = fill.Character;
	ci.Attributes = COMMON_LVB_LEADING_BYTE | COMMON_LVB_TRAILING_BYTE | (WORD)fill.ForegroundColor | ((WORD)fill.BackgroundColor << 4);
	try
	{
		for(int k = 0; k < total; ++k)
			buf[k] = ci;
		if (!WriteConsoleOutput(hStdout, buf, bufSize, bufOrigin, &rect))
			ThrowWithLastError("WriteConsoleOutput");
	}
	finally
	{
		delete[] buf;
	}
}

void FarUI::Break()
{
	INPUT_RECORD rec;
	DWORD writeCount;

	rec.EventType = KEY_EVENT;
	rec.Event.KeyEvent.bKeyDown = 1;
	rec.Event.KeyEvent.wRepeatCount = 1;
	rec.Event.KeyEvent.wVirtualKeyCode = VK_CANCEL;
	rec.Event.KeyEvent.wVirtualScanCode = (WORD)MapVirtualKeyA(rec.Event.KeyEvent.wVirtualKeyCode, 0);
	rec.Event.KeyEvent.uChar.UnicodeChar = rec.Event.KeyEvent.uChar.AsciiChar = 0;
	rec.Event.KeyEvent.dwControlKeyState = LEFT_CTRL_PRESSED;

	WriteConsoleInput(::GetStdHandle(STD_INPUT_HANDLE), &rec, 1, &writeCount);
}

//! Console Write() writes some Unicode chars as '?'.
//! Used to call SaveUserScreen() in the end. It was very slow. Now it is done in many places, see _100514_000000.
void FarUI::Write(String^ text)
{
	if (ES(text))
		return;

	if (!ValueUserScreen::Get()) //_100514_000000
	{
		ValueUserScreen::Set(true);
		ShowUserScreen();
	}

	PIN_NE(pin, text);
	DWORD cch = text->Length;
	WriteConsole(GetStdHandle(STD_OUTPUT_HANDLE), pin, cch, &cch, nullptr);
}

void FarUI::Write(String^ text, ConsoleColor foregroundColor)
{
	ConsoleColor fc = Console::ForegroundColor;
	Console::ForegroundColor = foregroundColor;
	Write(text);
	Console::ForegroundColor = fc;
}

void FarUI::Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
{
	ConsoleColor fc = Console::ForegroundColor;
	ConsoleColor bc = Console::BackgroundColor;
	Console::ForegroundColor = foregroundColor;
	Console::BackgroundColor = backgroundColor;
	Write(text);
	Console::ForegroundColor = fc;
	Console::BackgroundColor = bc;
}

void FarUI::ShowUserScreen()
{
	Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, 0, 0);
}

void FarUI::SaveUserScreen()
{
	Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, 0, 0);
}

void FarUI::SetProgressFlash()
{
	Info.AdvControl(&MainGuid, ACTL_PROGRESSNOTIFY, 0, 0);
}

void FarUI::SetProgressState(TaskbarProgressBarState state)
{
	Info.AdvControl(&MainGuid, ACTL_SETPROGRESSSTATE, (int)state, 0);
}

void FarUI::SetProgressValue(int currentValue, int maximumValue)
{
	ProgressValue arg;
	arg.Completed = currentValue;
	arg.Total = maximumValue;
	Info.AdvControl(&MainGuid, ACTL_SETPROGRESSVALUE, 0, &arg);
}

int FarUI::SaveScreen(int x1, int y1, int x2, int y2)
{
	return (int)(INT_PTR)Info.SaveScreen(x1, y1, x2, y2);
}

void FarUI::RestoreScreen(int screen)
{
	Info.RestoreScreen((HANDLE)(INT_PTR)screen);
}

void FarUI::Draw()
{
	Info.Text(0, 0, 0, nullptr);
}

void FarUI::DrawColor(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, String^ text)
{
	FarColor arg;
	arg.Flags = FCF_4BITMASK;
	arg.ForegroundColor = (COLORREF)foregroundColor;
	arg.BackgroundColor = (COLORREF)backgroundColor;

	PIN_NE(pin, text);
	
	Info.Text(left, top, &arg, pin);
}

void FarUI::DrawPalette(int left, int top, PaletteColor paletteColor, String^ text)
{
	FarColor arg = ::GetFarPaletteColor(paletteColor);
	PIN_NE(pin, text);
	Info.Text(left, top, &arg, pin);
}

ConsoleColor FarUI::GetPaletteBackground(PaletteColor paletteColor)
{
	FarColor arg = ::GetFarPaletteColor(paletteColor);
	return (ConsoleColor)arg.BackgroundColor;
}

ConsoleColor FarUI::GetPaletteForeground(PaletteColor paletteColor)
{
	FarColor arg = ::GetFarPaletteColor(paletteColor);
	return (ConsoleColor)arg.ForegroundColor;
}

IntPtr FarUI::MainWindowHandle::get()
{
	return (IntPtr)Info.AdvControl(&MainGuid, ACTL_GETFARHWND, 0, 0);
}

void FarUI::Clear()
{
	Console::Clear();
	SaveUserScreen();
}

void FarUI::Redraw()
{
	Info.AdvControl(&MainGuid, ACTL_REDRAWALL, 0, 0);
}

int FarUI::ReadKeys(array<KeyData^>^ keys)
{
	int result = -1;
	while (KeyAvailable)
	{
		KeyInfo^ info = ReadKey(Works::ReadKeyOptions::AllowCtrlC | Works::ReadKeyOptions::IncludeKeyDown | Works::ReadKeyOptions::IncludeKeyUp | Works::ReadKeyOptions::NoEcho);
		if (!keys || keys->Length == 0)
			break;
		
		KeyData key(info->VirtualKeyCode, info->CtrlAltShift());
		for(int i = 0; i < keys->Length; ++i)
		{
			if (keys[i] == %key)
			{
				result = i;
				break;
			}
		}
	}
	FlushInputBuffer();
	return result;
}

}
