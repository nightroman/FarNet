/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "RawUI.h"

namespace FarNet
{;
//???
void ThrowWithLastError(String^ msg)
{
	throw gcnew OperationCanceledException(msg + " error code: " + GetLastError());
}

void RawUI::FlushInputBuffer()
{
	HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);
	if (hStdin == INVALID_HANDLE_VALUE)
		ThrowWithLastError("GetStdHandle");

	if (!FlushConsoleInputBuffer(hStdin))
		ThrowWithLastError("FlushConsoleInputBuffer");
}

KeyInfo RawUI::ReadKey(ReadKeyOptions options)
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

void RawUI::ScrollBufferContents(Place source, Point destination, Place clip, BufferCell fill)
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

array<BufferCell,2>^ RawUI::GetBufferContents(Place rectangle)
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
		BufferCell empty(' ', Console::ForegroundColor, Console::BackgroundColor, BufferCellType::Complete);
		array<BufferCell, 2>^ r = gcnew array<BufferCell, 2>(h1, w1);
		for(int i = 0, k = 0; i < h2; ++i)
		{
			for(int j = 0; j < w2; ++j)
			{
				CHAR_INFO& ci = buf[k];
				r[i, j] = BufferCell(ci.Char.UnicodeChar, (ConsoleColor)(ci.Attributes & 0xF), (ConsoleColor)((ci.Attributes & 0xF0) >> 4), BufferCellType::Complete);
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

void RawUI::SetBufferContents(Point origin, array<BufferCell,2>^ contents)
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
				BufferCell fill = contents[i, j];
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

void RawUI::SetBufferContents(Place rectangle, BufferCell fill)
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
}
