#include "StdAfx.h"
#include "Dialog.h"
#include "Editor0.h"
#include "Viewer0.h"
#include "Panel0.h"
#include "Window.h"
#include "Wrappers.h"

namespace FarNet
{
IFace^ Window::GetAt(int index)
{
	WindowInfo wi;
	Call_ACTL_GETWINDOWINFO(wi, index);

	switch (wi.Type)
	{
	case WTYPE_DIALOG:
		return FarDialog::GetDialog(wi.Id);

	case WTYPE_EDITOR:
		return Editor0::GetEditor(wi.Id);

	case WTYPE_VIEWER:
		return Viewer0::GetViewer(wi.Id);
	}

	return Panel0::GetPanel(true);
}

IntPtr Window::GetIdAt(int index)
{
	WindowInfo wi;
	Call_ACTL_GETWINDOWINFO(wi, index);

	return (IntPtr)wi.Id;
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

	CBox box(wi.NameSize);
	wi.Name = box;

	Call_ACTL_GETWINDOWINFO(wi);

	return gcnew String(box);
}

int Window::Count::get()
{
	return Call_ACTL_GETWINDOWCOUNT();
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
	//_141017_151021 Far 3.0.4138 Not documented: -1 is for Panels.
	if (index == -1)
	{
		// find index of Panels
		int nWindow = Count;
		for(int iWindow = 0; iWindow < nWindow; ++iWindow)
		{
			WindowKind kind = GetKindAt(iWindow);
			if (kind == WindowKind::Panels)
			{
				index = iWindow;
				break;
			}
		}

		// not found
		if (index == -1)
			throw gcnew InvalidOperationException(__FUNCTION__ " failed, missing Panels");
	}

	if (!Info.AdvControl(&MainGuid, ACTL_SETCURRENTWINDOW, index, 0))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed, index = " + index);
}

}
