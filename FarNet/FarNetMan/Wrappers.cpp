/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Wrappers.h"

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

#undef FCTL_FREEPANELITEM
#undef FCTL_GETPANELITEM
#undef FCTL_GETSELECTEDPANELITEM

AutoPluginPanelItem::AutoPluginPanelItem(HANDLE handle, int index) : _handle(handle)
{
	if (!Info.Control(handle, FCTL_GETPANELITEM, index, (LONG_PTR)this))
		throw gcnew OperationCanceledException("Cannot get panel item; index: " + index);
}

AutoPluginPanelItem::~AutoPluginPanelItem()
{
	Info.Control(_handle, FCTL_FREEPANELITEM, 0, (LONG_PTR)this);
}

AutoSelectedPluginPanelItem::AutoSelectedPluginPanelItem(HANDLE handle, int index) : _handle(handle)
{
	if (!Info.Control(handle, FCTL_GETSELECTEDPANELITEM, index, (LONG_PTR)this))
		throw gcnew OperationCanceledException("Cannot get selected panel item; index: " + index);
}

AutoSelectedPluginPanelItem::~AutoSelectedPluginPanelItem()
{
	Info.Control(_handle, FCTL_FREEPANELITEM, 0, (LONG_PTR)this);
}
