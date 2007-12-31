/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#include "StdAfx.h"
#include "Viewer.h"
#include "Utils.h"

#define SET_BIT(VAR, VALUE, FLAG) if (VALUE) VAR |= FLAG; else VAR &= ~FLAG;

namespace FarNet
{;
Viewer::Viewer()
{
	_id = -1;
	_flags = VF_NONMODAL;
	_title = String::Empty;
}

void Viewer::Open()
{
	AssertClosed();

	CBox sFileName(_fileName);
	CBox sTitle(_title);

	int res = Info.Viewer(sFileName, sTitle, _window.Left, _window.Top, _window.Right, _window.Bottom, _flags);

	// check errors
	if ((_flags & VF_NONMODAL) == 0 && res == FALSE)
		throw gcnew OperationCanceledException("Can't open file: " + _fileName);

	// tmp
	_id = 0;
}

bool Viewer::Async::get()
{
	return (_flags & VF_IMMEDIATERETURN) != 0;
}

void Viewer::Async::set(bool value)
{
	AssertClosed();
	SET_BIT(_flags, value, VF_IMMEDIATERETURN);
}

bool Viewer::DeleteOnClose::get()
{
	return (_flags & VF_DELETEONCLOSE) != 0;
}

void Viewer::DeleteOnClose::set(bool value)
{
	AssertClosed();
	SET_BIT(_flags, value, VF_DELETEONCLOSE);
}

bool Viewer::DeleteOnlyFileOnClose::get()
{
	return (_flags & VF_DELETEONLYFILEONCLOSE) != 0;
}

void Viewer::DeleteOnlyFileOnClose::set(bool value)
{
	AssertClosed();
	SET_BIT(_flags, value, VF_DELETEONLYFILEONCLOSE);
}

bool Viewer::EnableSwitch::get()
{
	return (_flags & VF_ENABLE_F6) != 0;
}

void Viewer::EnableSwitch::set(bool value)
{
	AssertClosed();
	SET_BIT(_flags, value, VF_ENABLE_F6);
}

bool Viewer::DisableHistory::get()
{
	return (_flags & VF_DISABLEHISTORY) != 0;
}

void Viewer::DisableHistory::set(bool value)
{
	AssertClosed();
	SET_BIT(_flags, value, VF_DISABLEHISTORY);
}

bool Viewer::IsModal::get()
{
	return (_flags & VF_NONMODAL) == 0;
}

void Viewer::IsModal::set(bool value)
{
	AssertClosed();
	value = !value;
	SET_BIT(_flags, value, VF_NONMODAL);
}

bool Viewer::IsOpened::get()
{
	return _id >= 0;
}

String^ Viewer::FileName::get()
{
	return _fileName;
}

void Viewer::FileName::set(String^ value)
{
	AssertClosed();
	_fileName = value;
}

String^ Viewer::Title::get()
{
	return _title;
}

void Viewer::Title::set(String^ value)
{
	AssertClosed();
	_title = value;
}

Place Viewer::Window::get()
{
	if (IsOpened)
		GetParams();
	return _window;
}

void Viewer::Window::set(Place value)
{
	_window = value;
}

void Viewer::AssertClosed()
{
	if (IsOpened) throw gcnew InvalidOperationException("Viewer must not be open for this operation.");
}

void Viewer::GetParams()
{
	ViewerInfo vi; ViewerControl_VCTL_GETINFO(vi);
	_window.Top = 0;
	_window.Left = 0;
	_window.Width = vi.WindowSizeX;
	_window.Height = vi.WindowSizeY;
	_fileName = OemToStr(vi.FileName);
}

}
