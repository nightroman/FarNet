/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Panel1;
ref class Panel2;

ref class ShelveInfoPanel sealed : Works::ShelveInfo
{
public:
	static ShelveInfoPanel^ CreateActiveInfo(bool modes);
public:
	ShelveInfoPanel(Panel1^ panel, bool modes);
	virtual property String^ Title { String^ get() override { return Path; } }
	virtual void Pop() override;
public:
	property String^ Path;
	property String^ Current;
private:
	bool _modes;
	bool _sortDesc;
	PanelSortMode _sortMode;
	PanelViewMode _viewMode;
};

ref class ShelveInfoPlugin sealed : Works::ShelveInfo
{
public:
	ShelveInfoPlugin(Panel2^ plugin);
	property Panel2^ Panel { Panel2^ get() { return _plugin; } }
	virtual property String^ Title { String^ get() override; }
	virtual void Pop() override;
private:
	Panel2^ _plugin;
};
}
