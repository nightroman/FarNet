/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Zoo.h"
#include "Shelve.h"

namespace FarNet
{;
void ThrowWithLastError(String^ msg)
{
	throw gcnew OperationCanceledException(msg + " error code: " + GetLastError());
}

void Zoo::FlushInputBuffer()
{
	HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);
	if (hStdin == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	if (!FlushConsoleInputBuffer(hStdin))
		ThrowWithLastError("FlushConsoleInputBuffer");
}

KeyInfo Zoo::ReadKey(Works::ReadKeyOptions options)
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
			return KeyInfo(
				ir.Event.KeyEvent.wVirtualKeyCode,
				ir.Event.KeyEvent.uChar.UnicodeChar,
				(ControlKeyStates)ir.Event.KeyEvent.dwControlKeyState,
				ir.Event.KeyEvent.bKeyDown != 0);
		}
	}
	finally
	{
		SetConsoleMode(hStdin, mode1);
	}
}

void Zoo::ScrollBufferContents(Place source, Point destination, Place clip, Works::BufferCell fill)
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

array<Works::BufferCell, 2>^ Zoo::GetBufferContents(Place rectangle)
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

void Zoo::SetBufferContents(Point origin, array<Works::BufferCell, 2>^ contents)
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

void Zoo::SetBufferContents(Place rectangle, Works::BufferCell fill)
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

// _091007_034112
String^ Zoo::ConsoleTitle::get()
{
	// .NET uses buf[0x5fb5] size
	// CA: C6262: Function uses '49008' bytes of stack: exceeds /analyze:stacksize'16384'. Consider moving some data to heap.
	wchar_t buf[4096];
	if (::GetConsoleTitle(buf, countof(buf)))
		return gcnew String(buf);

	return String::Empty;
}

Object^ Zoo::Shelve::get()
{
	return %ShelveInfo::_stack;
}

void Zoo::Break()
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

MacroParseError^ Zoo::CheckMacro(String^ sequence, bool silent)
{
	PIN_ES(pin, sequence);
	
	ActlKeyMacro args;
	args.Command = MCMD_CHECKMACRO;
	args.Param.PlainText.SequenceText = pin;
	args.Param.PlainText.Flags = silent ? KSFLAGS_SILENTCHECK : 0;

	//! it always gets ErrCode
	Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &args);
	if (args.Param.MacroResult.ErrCode == MPEC_SUCCESS)
		return nullptr;
	
	MacroParseError^ r = gcnew MacroParseError;
	r->ErrorCode = (MacroParseStatus)args.Param.MacroResult.ErrCode;
	r->Token = gcnew String(args.Param.MacroResult.ErrSrc);
	r->Line = args.Param.MacroResult.ErrPos.Y;
	r->Pos = args.Param.MacroResult.ErrPos.X;
	return r;
}

void Zoo::LoadMacros()
{
	ActlKeyMacro args;
	args.Command = MCMD_LOADALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &args))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void Zoo::SaveMacros()
{
	ActlKeyMacro args;
	args.Command = MCMD_SAVEALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &args))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

}
