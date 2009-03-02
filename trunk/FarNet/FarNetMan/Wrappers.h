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

class AutoPluginPanelItem
{
public:
	AutoPluginPanelItem(HANDLE handle, int index, bool selected);
	~AutoPluginPanelItem();
	const PluginPanelItem& Get() const { return *m; }
private:
	PluginPanelItem* m;
	char mBuffer[1024];
	void operator=(const AutoPluginPanelItem&) {}
};
