
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "Wrappers.h"
#include "farcolor.hpp"

// STOP
// How it fails: open Vessel history dialog, [F3] ~ open viewer, [Esc] ~ close viewer
// => VE_CLOSE => Vessel's handler calls ACTL_GETWINDOWTYPE and it returns 0.
// Far2 and Far3 both have the same issue. WindowKind::None will do.
WindowKind Wrap::WindowGetKind()
{
	WindowType arg;
	arg.StructSize = sizeof(WindowType);
	if (Info.AdvControl(&MainGuid, ACTL_GETWINDOWTYPE, 0, &arg))
		return (WindowKind)arg.Type;
	else
		return WindowKind::None;
}

int Wrap::GetEndPalette()
{
	return COL_LASTPALETTECOLOR;
}

/*
GetPanelInfo is in progress. The problem: Far calls AsGetOpenPluginInfo and plugin may have UpdateInfo handler
where it may call panel properties, i.e. GetPanelInfo again. Far resolves recursion fine, but there are issues.
E.g. UpdateInfo asks the current file; UpdateFiles removes items and asks view mode - the latter triggers
UpdateInfo and returned current file info is inconsistent with the actual items.
NOTE: Far design flaw.
*/
bool State::GetPanelInfo;

#undef ECTL_GETINFO

AutoEditorInfo::AutoEditorInfo(bool safe)
{
	if (!Info.EditorControl(-1, ECTL_GETINFO, 0, this))
	{
		if (safe)
			EditorID = -1;
		else
			throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
	}
}

void AutoEditorInfo::Update()
{
	if (!Info.EditorControl(-1, ECTL_GETINFO, 0, this))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
}

AutoPluginPanelItem::AutoPluginPanelItem(HANDLE handle, int index, FileType type)
{
	m.Size = Info.PanelControl(handle, (FILE_CONTROL_COMMANDS)type, index, 0);
	if (m.Size > sizeof(mBuffer))
		m.Item = (PluginPanelItem*)new char[m.Size];
	else
		m.Item = (PluginPanelItem*)mBuffer;

	try
	{
		if (!Info.PanelControl(handle, (FILE_CONTROL_COMMANDS)type, index, &m))
			throw gcnew InvalidOperationException("Cannot get panel item; index: " + index);
	}
	catch(...)
	{
		if (mBuffer != (char*)m.Item)
			delete[] (char*)m.Item;

		throw;
	}
}

AutoPluginPanelItem::~AutoPluginPanelItem()
{
	if (mBuffer != (char*)m.Item)
		delete[] (char*)m.Item;
}

#undef FCTL_GETPANELINFO

void GetPanelInfo(HANDLE handle, PanelInfo& info)
{
	SetState<bool> state(State::GetPanelInfo, true);

	info.StructSize = sizeof(info);
	if (!Info.PanelControl(handle, FCTL_GETPANELINFO, 0, &info))
		throw gcnew InvalidOperationException("Cannot get panel information.");
}

//! Steps: open a panel; Tab; CtrlL; $Far.Panel used to fail
bool TryPanelInfo(HANDLE handle, PanelInfo& info)
{
	SetState<bool> state(State::GetPanelInfo, true);

	info.StructSize = sizeof(info);
	return Info.PanelControl(handle, FCTL_GETPANELINFO, 0, &info) ? true : false;
}

// Gets dialog control text of any length
String^ GetDialogControlText(HANDLE hDlg, int id, int start, int len)
{
	const wchar_t* sz = (const wchar_t*)Info.SendDlgMessage(hDlg, DM_GETCONSTTEXTPTR, id, 0);
	if (start >= 0)
		return gcnew String(sz, start, len);
	else
		return gcnew String(sz);
}
