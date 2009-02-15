/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

class AutoWindowInfo : public WindowInfo
{
public:
	AutoWindowInfo(int index);
	~AutoWindowInfo();
private:
	void operator=(const AutoWindowInfo&) {}
};

class AutoEditorInfo : public EditorInfo
{
public:
	AutoEditorInfo(bool safe = false);
	~AutoEditorInfo();
	void Update();
private:
	void operator=(const AutoEditorInfo&) {}
};

class AutoPluginPanelItem : public PluginPanelItem
{
public:
	AutoPluginPanelItem(HANDLE handle, int index);
	~AutoPluginPanelItem();
private:
	const HANDLE _handle;
	void operator=(const AutoPluginPanelItem&) {}
};

class AutoSelectedPluginPanelItem : public PluginPanelItem
{
public:
	AutoSelectedPluginPanelItem(HANDLE handle, int index);
	~AutoSelectedPluginPanelItem();
private:
	const HANDLE _handle;
	void operator=(const AutoSelectedPluginPanelItem&) {}
};
