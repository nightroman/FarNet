
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#include "StdAfx.h"
#include "Shelve.h"
#include "Panel2.h"

namespace FarNet
{;
ShelveInfoNative::ShelveInfoNative(Panel1^ panel, bool modes)
: _modes(modes)
{
	// case: special panel, e.g. QView.
	// Let's use the active path to restore, no path (just close) is worse.
	if (panel->Kind != PanelKind::File)
	{
		Path = Far::Api->CurrentDirectory;
		return;
	}

	// file panel, path
	Path = panel->CurrentDirectory;

	// current name
	FarFile^ file = panel->CurrentFile;
	if (file)
		Current = file->Name;

	// selected names
	InitSelectedNames(panel);

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

ShelveInfoNative^ ShelveInfoNative::CreateActiveInfo(bool modes)
{
	// any panel
	IPanel^ native = Far::Api->Panel;
	if (!native)
		return nullptr;

	// module panel
	Panel^ module = dynamic_cast<Panel^>(native); 
	if (module)
		return ((Panel2^)module->WorksPanel)->_ActiveInfo;

	// native plugin panel; it is closed if another panel is opened
	if (native->IsPlugin) //_110201_111328
		return nullptr;
	
	// must be a file system panel
	return gcnew ShelveInfoNative((Panel1^)native, modes);
}

// _110313_054719 Now works for passive, too.
void ShelveInfoNative::Pop(bool active)
{
	HANDLE handle = active ? PANEL_ACTIVE : PANEL_PASSIVE;
	if (Path)
		::SetPanelDirectory(handle, Path);

	array<String^>^ selectedNames = GetSelectedNames();
	if (!Current && !selectedNames && !_modes)
		return;

	PanelInfo pi;
	GetPanelInfo(handle, pi);
	if (0 != (pi.Flags & PFLAGS_PLUGIN) || pi.PanelType != PTYPE_FILEPANEL)
		//! do not throw, sometimes a panel just cannot close
		return;

	Panel1 native(active);

	if (Current)
		native.GoToName(Current);

	native.SelectNames(selectedNames);

	// restore modes
	if (_modes)
	{
		if (_viewMode != (PanelViewMode)pi.ViewMode)
			native.ViewMode = _viewMode;
		
		bool reversed = (pi.Flags & PFLAGS_REVERSESORTORDER) != 0;
		PanelSortMode sortMode = (PanelSortMode)(reversed ? -pi.SortMode : pi.SortMode);
		if (_sortMode != sortMode)
			native.SortMode = _sortMode;
	}
}

ShelveInfoModule::ShelveInfoModule(Panel2^ panel)
: _panel(panel)
{
	InitSelectedIndexes(panel);
}

String^ ShelveInfoModule::Title::get()
{
	return JoinText(_panel->Title, _panel->CurrentLocation);
}

// _110313_054719 Still does not support passive.
void ShelveInfoModule::Pop(bool active)
{
	Log::Source->TraceInformation(__FUNCTION__);
	if (!active) throw gcnew NotSupportedException("Passive panel is not supported");
	
	_panel->Open();
	_panel->_postSelected = GetSelectedIndexes();
}

}
