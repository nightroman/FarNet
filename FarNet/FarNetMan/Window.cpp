
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
int Window::Count::get()
{
	return (int)Info.AdvControl(&MainGuid, ACTL_GETWINDOWCOUNT, 0, 0);
}

WindowKind Window::Kind::get()
{
	return Wrap::WindowGetKind();
}

WindowKind Window::GetKindAt(int index)
{
	WindowInfo wi;
	wi.StructSize = sizeof(wi);
	wi.Name = wi.TypeName = nullptr;
	wi.NameSize = wi.TypeNameSize = 0;
	
	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, index, &wi))
		throw gcnew InvalidOperationException("ACTL_GETWINDOWINFO: " + index + " failed.");

	return (FarNet::WindowKind)wi.Type;
}

String^ Window::GetKindNameAt(int index)
{
	WindowInfo wi;
	wi.StructSize = sizeof(wi);
	wi.Name = wi.TypeName = nullptr;
	wi.NameSize = wi.TypeNameSize = 0;
	
	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, index, &wi))
		throw gcnew InvalidOperationException("ACTL_GETWINDOWINFO: " + index + " failed.");

	CBox text(wi.TypeNameSize);
	wi.TypeName = text;

	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, index, &wi))
		throw gcnew InvalidOperationException("ACTL_GETWINDOWINFO: " + index + " failed.");

	return gcnew String(text);
}

String^ Window::GetNameAt(int index)
{
	WindowInfo wi;
	wi.StructSize = sizeof(wi);
	wi.Name = wi.TypeName = nullptr;
	wi.NameSize = wi.TypeNameSize = 0;
	
	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, index, &wi))
		throw gcnew InvalidOperationException("ACTL_GETWINDOWINFO: " + index + " failed.");

	CBox text(wi.NameSize);
	wi.Name = text;

	if (!Info.AdvControl(&MainGuid, ACTL_GETWINDOWINFO, index, &wi))
		throw gcnew InvalidOperationException("ACTL_GETWINDOWINFO: " + index + " failed.");

	return gcnew String(text);
}

void Window::SetCurrentAt(int index)
{
	if (!Info.AdvControl(&MainGuid, ACTL_SETCURRENTWINDOW, index, 0))
		throw gcnew InvalidOperationException("SetCurrentWindow:" + index + " failed.");
}

bool Window::Commit()
{
	return Info.AdvControl(&MainGuid, ACTL_COMMIT, 0, 0) != 0;
}

}
