/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Shelve.h"
#include "Panel2.h"

namespace FarNet
{;
ShelveInfoPanel::ShelveInfoPanel(Panel1^ panel, bool modes)
: _modes(modes)
{
	// case: special panel, e.g. QView.
	// Let's use the active path to restore, no path (just close) is worse.
	if (panel->Kind != PanelKind::File)
	{
		Path = Far::Net->ActivePath;
		return;
	}

	// file panel, path
	Path = panel->Path;

	// current name
	FarFile^ file = panel->CurrentFile;
	if (file)
		Current = file->Name;

	// selected names
	InitSelectedNames(panel, Current);

	// modes
	if (!modes)
		return;

	// get info
	PanelInfo pi;
	GetPanelInfo(panel->Handle, pi);

	// store modes
	bool reversed = (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
	_sortMode = (PanelSortMode)(reversed ? -pi.SortMode : pi.SortMode);
	_viewMode = (PanelViewMode)pi.ViewMode;
}

ShelveInfoPanel^ ShelveInfoPanel::CreateActiveInfo(bool modes)
{
	// any panel
	IAnyPanel^ panel = Far::Net->Panel;
	if (!panel)
		return nullptr;

	// FarNet panel
	Panel2^ panelFarNet = dynamic_cast<Panel2^>(panel); 
	if (panelFarNet)
		return panelFarNet->_ActiveInfo;

	// native plugin panel; it is closed if another panel is opened
	if (panel->IsPlugin) //_110201_111328
		return nullptr;
	
	// must be a file system panel
	return gcnew ShelveInfoPanel((Panel1^)panel, modes);
}

// NOW: works only for the active panel.
void ShelveInfoPanel::Pop()
{
	if (Path)
	{
		PIN_NE(pin, Path);
		if (!Info.Control(PANEL_ACTIVE, FCTL_SETPANELDIR, 0, (LONG_PTR)pin))
			throw gcnew InvalidOperationException("Cannot set panel directory: " + Path);
	}

	array<String^>^ selectedNames = GetSelectedNames();
	if (!Current && !selectedNames && !_modes)
		return;

	PanelInfo pi;
	GetPanelInfo(PANEL_ACTIVE, pi);
	if (pi.Plugin || pi.PanelType != PTYPE_FILEPANEL)
		//! do not throw, sometimes a panel just cannot close
		return;

	Panel1 panel(true);

	if (Current)
		panel.GoToName(Current);

	panel.SelectNames(selectedNames);

	// restore modes
	if (_modes)
	{
		if (_viewMode != (PanelViewMode)pi.ViewMode)
			panel.ViewMode = _viewMode;
		
		bool reversed = (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
		PanelSortMode sortMode = (PanelSortMode)(reversed ? -pi.SortMode : pi.SortMode);
		if (_sortMode != sortMode)
			panel.SortMode = _sortMode;
	}
}

ShelveInfoPlugin::ShelveInfoPlugin(Panel2^ plugin)
: _plugin(plugin)
{
	InitSelectedIndexes(plugin);
}

String^ ShelveInfoPlugin::Title::get()
{
	return JoinText(_plugin->_info.Title, _plugin->_info.CurrentDirectory);
}

void ShelveInfoPlugin::Pop()
{
	Log::Source->TraceInformation(__FUNCTION__);
	_plugin->Open();
	_plugin->_postSelected = GetSelectedIndexes();
}

}
