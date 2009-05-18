/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Wrappers.h"

/*
GetPanelInfo is in progress. The problem: Far calls AsGetOpenPluginInfo and plugin may have GettingInfo handler
where it may call panel properties, i.e. GetPanelInfo again. Far resolves recursion fine, but there are issues.
E.g. GettingInfo asks the current file; GettingData removes items and asks view mode - the latter triggers
GettingInfo and returned current file info is inconsistent with the actual items.
NOTE: Far design flaw.
*/
bool State::GetPanelInfo;

#undef ACTL_GETWINDOWINFO
#undef ACTL_FREEWINDOWINFO

AutoWindowInfo::AutoWindowInfo(int index)
{
	Pos = index;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_GETWINDOWINFO, this))
		throw gcnew InvalidOperationException("GetWindowInfo:" + index + " failed.");
}

AutoWindowInfo::~AutoWindowInfo()
{
	Info.AdvControl(Info.ModuleNumber, ACTL_FREEWINDOWINFO, this);
}

#undef ECTL_GETINFO
#undef ECTL_FREEINFO

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

AutoEditorInfo::~AutoEditorInfo()
{
	Info.EditorControl(ECTL_FREEINFO, this);
}

void AutoEditorInfo::Update()
{
	Info.EditorControl(ECTL_FREEINFO, this);
	if (!Info.EditorControl(ECTL_GETINFO, this))
		throw gcnew InvalidOperationException(__FUNCTION__ " failed. Ensure current editor.");
}

#undef FCTL_GETPANELITEM
#undef FCTL_GETSELECTEDPANELITEM

AutoPluginPanelItem::AutoPluginPanelItem(HANDLE handle, int index, bool selected)
{
	const int type = selected ? FCTL_GETSELECTEDPANELITEM : FCTL_GETPANELITEM;
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
