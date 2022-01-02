
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
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

KeyInfo^ FarUI::ReadKey(ReadKeyOptions options)
{
	if (int(options & (ReadKeyOptions::IncludeKeyDown | ReadKeyOptions::IncludeKeyUp)) == 0)
		throw gcnew ArgumentException("Argument 'options': either IncludeKeyDown, IncludeKeyUp or both must be set.");

	HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);
	if (hStdin == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	DWORD mode1;
	if (!GetConsoleMode(hStdin, &mode1))
		ThrowWithLastError("GetConsoleMode");

	DWORD mode2 = 0;
	if (int(options & ReadKeyOptions::NoEcho) == 0)
		mode2 |= (ENABLE_ECHO_INPUT | ENABLE_LINE_INPUT);
	if (int(options & ReadKeyOptions::AllowCtrlC) != 0)
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
				if (int(options & ReadKeyOptions::IncludeKeyDown) == 0)
					continue;
			}
			else
			{
				if (int(options & ReadKeyOptions::IncludeKeyUp) == 0)
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

	auto buf = std::make_unique<CHAR_INFO[]>(w1 * h1);
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

	if (!ReadConsoleOutput(hStdout, buf.get(), coordBufSize, coordBufCoord, &srctReadRect))
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

	auto buf = std::make_unique<CHAR_INFO[]>(bufSize.X * bufSize.Y);
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
	if (!WriteConsoleOutput(hStdout, buf.get(), bufSize, bufOrigin, &rect))
		ThrowWithLastError("WriteConsoleOutput");
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

	int total = bufSize.X * bufSize.Y;
	auto buf = std::make_unique<CHAR_INFO[]>(total);
	CHAR_INFO ci;
	ci.Char.UnicodeChar = fill.Character;
	ci.Attributes = COMMON_LVB_LEADING_BYTE | COMMON_LVB_TRAILING_BYTE | (WORD)fill.ForegroundColor | ((WORD)fill.BackgroundColor << 4);
	for(int k = 0; k < total; ++k)
		buf[k] = ci;
	if (!WriteConsoleOutput(hStdout, buf.get(), bufSize, bufOrigin, &rect))
		ThrowWithLastError("WriteConsoleOutput");
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
void FarUI::Write(String^ text)
{
	if (ES(text))
		return;

	ShowUserScreen();

	PIN_NE(pin, text);
	DWORD cch = text->Length;
	WriteConsole(GetStdHandle(STD_OUTPUT_HANDLE), pin, cch, &cch, nullptr);
}

//! set colors after user screen
void FarUI::Write(String^ text, ConsoleColor foregroundColor)
{
	if (ES(text))
		return;

	ShowUserScreen();

	ConsoleColor fc = Console::ForegroundColor;
	Console::ForegroundColor = foregroundColor;
	Write(text);
	Console::ForegroundColor = fc;
}

//! set colors after user screen
void FarUI::Write(String^ text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
{
	if (ES(text))
		return;

	ShowUserScreen();

	ConsoleColor fc = Console::ForegroundColor;
	ConsoleColor bc = Console::BackgroundColor;
	Console::ForegroundColor = foregroundColor;
	Console::BackgroundColor = backgroundColor;
	Write(text);
	Console::ForegroundColor = fc;
	Console::BackgroundColor = bc;
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
	ProgressValue arg = {sizeof(arg)};
	arg.Completed = currentValue;
	arg.Total = maximumValue;
	Info.AdvControl(&MainGuid, ACTL_SETPROGRESSVALUE, 0, &arg);
}

IntPtr FarUI::SaveScreen(int x1, int y1, int x2, int y2)
{
	return (IntPtr)Info.SaveScreen(x1, y1, x2, y2);
}

void FarUI::RestoreScreen(IntPtr screen)
{
	Info.RestoreScreen((HANDLE)screen);
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

//_140317_201247
// Why cursor. With Far /s all is fine. Else on `cls` in PSF Command Console
// the cursor is somewhere and not shown in the prompt until click or type.
// FarNet.5.2.2 - removed cursor stuff.
void FarUI::Clear()
{
	ShowUserScreen();
	Console::Clear();
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
		KeyInfo^ info = ReadKey(ReadKeyOptions::AllowCtrlC | ReadKeyOptions::IncludeKeyDown | ReadKeyOptions::IncludeKeyUp | ReadKeyOptions::NoEcho);
		if (!keys || keys->Length == 0)
			break;

		KeyData key(info->VirtualKeyCode, info->CtrlAltShift());
		for(int i = 0; i < keys->Length; ++i)
		{
			if (key.Equals(keys[i]))
			{
				result = i;
				break;
			}
		}
	}
	FlushInputBuffer();
	return result;
}

String^ FarUI::GetBufferLineText(int lineIndex)
{
	Point size = BufferSize;
	if (lineIndex < 0)
		lineIndex += BufferSize.Y;
	if (lineIndex < 0 || lineIndex >= BufferSize.Y)
		throw gcnew IndexOutOfRangeException("Buffer line index is out of range.");

	// get
	Place rect(0, lineIndex, BufferSize.X - 1, lineIndex);
	array<Works::BufferCell, 2>^ cells = GetBufferContents(rect);

	// find last
	int last = cells->GetLength(1);
	while(--last >= 0 && cells[0, last].Character == ' ') {}

	// empty
	if (last < 0)
		return String::Empty;

	StringBuilder sb(last + 1);
	for (int i = 0; i <= last; ++i)
		sb.Append(cells[0, i].Character, 1);

	return sb.ToString();
}

bool isConsoleModal()
{
	WindowInfo wi;
	int index = -1;

	// find index of Desktop
	int nWindow = Far::Api->Window->Count;
	int iWindow = 0;
	do
	{
		Call_ACTL_GETWINDOWINFO(wi, iWindow);
		if (wi.Type == WTYPE_DESKTOP)
		{
			index = iWindow;
			break;
		}
	} while (++iWindow < nWindow);

	// not found
	if (index == -1)
		throw gcnew InvalidOperationException(__FUNCTION__ " failed, missing Desktop");

	return (wi.Flags & WIF_MODAL) != 0;
}


const intptr_t UserScreenNoNewLine = 1;

#pragma push_macro("FCTL_GETUSERSCREEN")
#undef FCTL_GETUSERSCREEN
void static GetUserScreen()
{
	ConsoleColor fc = Console::ForegroundColor;
	ConsoleColor bc = Console::BackgroundColor;
	Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_GETUSERSCREEN, UserScreenNoNewLine, 0);
	Console::ForegroundColor = fc;
	Console::BackgroundColor = bc;
}
#pragma pop_macro("FCTL_GETUSERSCREEN")

#pragma push_macro("FCTL_SETUSERSCREEN")
#undef FCTL_SETUSERSCREEN
void static SetUserScreen()
{
	Info.PanelControl(INVALID_HANDLE_VALUE, FCTL_SETUSERSCREEN, UserScreenNoNewLine, 0);
}
#pragma pop_macro("FCTL_SETUSERSCREEN")

void FarUI::ShowUserScreen()
{
	// only if not shown
	if (_UserScreenCount == 0)
		::GetUserScreen();

	// update always, for stats
	++_UserScreenCount;
}

void FarUI::SaveUserScreen()
{
	// only if done, assuming paired calls
	if (_UserScreenCount == 1)
		::SetUserScreen();

	// update but keep positive for reset
	if (_UserScreenCount > 0)
		--_UserScreenCount;
}

int FarUI::SetUserScreen(int level)
{
	int oldLevel = _UserScreenCount;
	_UserScreenCount = level;

	if (level == 0)
		::SetUserScreen();
	else
		::GetUserScreen();

	return oldLevel;
}

void FarUI::ResetUserScreen()
{
	// only if shown and not done
	if (_UserScreenCount > 0)
		::SetUserScreen();

	// reset
	_UserScreenCount = 0;
}
}
