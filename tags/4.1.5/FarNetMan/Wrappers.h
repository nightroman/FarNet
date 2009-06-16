/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

class State
{
public:
	static bool GetPanelInfo;
};

template<class Type>
class SetState
{
public:
	SetState(Type& value1, Type value2) : _value(&value1), _saved(value1)
	{
		*_value = value2;
	}
	~SetState()
	{
		*_value = _saved;
	}
private:
	Type* _value;
	Type _saved;
};

class AutoEditorInfo : public EditorInfo
{
public:
	AutoEditorInfo(bool safe = false);
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

void GetPanelInfo(HANDLE handle, PanelInfo& info);
bool TryPanelInfo(HANDLE handle, PanelInfo& info);
