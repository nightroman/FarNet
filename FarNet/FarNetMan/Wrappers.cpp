/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Wrappers.h"
#include "farcolor.hpp"
#include "farkeys.hpp"

int Wrap::GetEndKeyCode()
{
	return INTERNAL_KEY_BASE_2;
}

int Wrap::GetEndPalette()
{
	return COL_LASTPALETTECOLOR;
}

/*
GetPanelInfo is in progress. The problem: Far calls AsGetOpenPluginInfo and plugin may have GettingInfo handler
where it may call panel properties, i.e. GetPanelInfo again. Far resolves recursion fine, but there are issues.
E.g. GettingInfo asks the current file; GettingData removes items and asks view mode - the latter triggers
GettingInfo and returned current file info is inconsistent with the actual items.
NOTE: Far design flaw.
*/
bool State::GetPanelInfo;

#undef ECTL_GETINFO

AutoEditorInfo::AutoEditorInfo(bool safe)
{
	if (!Info.EditorControl(ECTL_GETINFO, this))
	{
		if (safe)
			EditorID = -1;
		else
			throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
	}
}

void AutoEditorInfo::Update()
{
	if (!Info.EditorControl(ECTL_GETINFO, this))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
}

AutoPluginPanelItem::AutoPluginPanelItem(HANDLE handle, int index, FileType type)
{
	const int size = Info.Control(handle, type, index, 0);
	if (size > sizeof(mBuffer))
		m = (PluginPanelItem*)new char[size];
	else
		m = (PluginPanelItem*)mBuffer;

	try
	{
		if (!Info.Control(handle, type, index, (LONG_PTR)m))
			throw gcnew OperationCanceledException("Cannot get panel item; index: " + index);
	}
	catch(...)
	{
		if (mBuffer != (char*)m)
			delete[] (char*)m;

		throw;
	}
}

AutoPluginPanelItem::~AutoPluginPanelItem()
{
	if (mBuffer != (char*)m)
		delete[] (char*)m;
}

#undef FCTL_GETPANELINFO

void GetPanelInfo(HANDLE handle, PanelInfo& info)
{
	SetState<bool> state(State::GetPanelInfo, true);

	if (!Info.Control(handle, FCTL_GETPANELINFO, 0, (LONG_PTR)&info))
		throw gcnew OperationCanceledException("Cannot get panel information.");
}

//! Steps: open a panel; Tab; CtrlL; $Far.Panel used to fail
bool TryPanelInfo(HANDLE handle, PanelInfo& info)
{
	SetState<bool> state(State::GetPanelInfo, true);

	return Info.Control(handle, FCTL_GETPANELINFO, 0, (LONG_PTR)&info) ? true : false;
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
