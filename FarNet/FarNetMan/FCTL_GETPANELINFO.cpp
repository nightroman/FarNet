#include "StdAfx.h"
#include "FCTL_GETPANELINFO.h"
#include "Wrappers.h"

#undef FCTL_GETPANELINFO

bool HasPanels()
{
	PanelInfo info;
	info.StructSize = sizeof(info);
	return (bool)Info.PanelControl(PANEL_ACTIVE, FCTL_GETPANELINFO, 0, &info);
}

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
