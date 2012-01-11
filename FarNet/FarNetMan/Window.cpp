
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "Window.h"
#include "Wrappers.h"

#undef ACTL_GETWINDOWINFO

namespace FarNet
{;
static void Call_ACTL_GETWINDOWINFO(WindowInfo& wi, int index)
{
	wi.StructSize = sizeof(wi);
	wi.TypeName = nullptr;
	wi.Name = nullptr;
	wi.TypeNameSize = 0;
	wi.NameSize = 0;
	wi.Pos = index;
	
	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, 0, &wi))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed, index = " + index);
}

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

	CBox text(wi.NameSize);
	wi.Name = text;
	
	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, 0, &wi))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed, index = " + index);

	return gcnew String(text);
}

int Window::Count::get()
{
	return (int)Info.AdvControl(&MainGuid, ACTL_GETWINDOWCOUNT, 0, 0);
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
