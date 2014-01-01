
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#include "StdAfx.h"
#include "Window.h"
#include "Wrappers.h"

namespace FarNet
{;
WindowKind Window::GetKindAt(int index)
{
	WindowInfo wi;
	Call_ACTL_GETWINDOWINFO(wi, index);

	return (FarNet::WindowKind)wi.Type;
}

String^ Window::GetNameAt(int index)
{
	WindowInfo wi;
	Call_ACTL_GETWINDOWINFO(wi, index);

	CBox box(wi.NameSize);
	wi.Name = box;

	Call_ACTL_GETWINDOWINFO(wi);

	return gcnew String(box);
}

int Window::Count::get()
{
	return (int)Info.AdvControl(&MainGuid, ACTL_GETWINDOWCOUNT, 0, 0);
}

bool Window::IsModal::get()
{
	WindowInfo wi;
	Call_ACTL_GETWINDOWINFO(wi, -1);

	return 0 != (wi.Flags & WIF_MODAL);
}

WindowKind Window::Kind::get()
{
	return Wrap::WindowGetKind();
}

void Window::SetCurrentAt(int index)
{
	if (!Info.AdvControl(&MainGuid, ACTL_SETCURRENTWINDOW, index, 0))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed, index = " + index);

	Info.AdvControl(&MainGuid, ACTL_COMMIT, 0, 0);
}

}
