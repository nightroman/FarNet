
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

#include "StdAfx.h"
#include "History.h"
#include "Settings.h"

namespace FarNet
{;
static HistoryInfo^ NewHistoryInfo(const FarSettingsHistory& that)
{
	return gcnew HistoryInfo(gcnew String(that.Name), FileTimeToDateTime(that.Time), that.Lock != 0);
}

array<HistoryInfo^>^ History::Command()
{
	Settings settings(FarGuid);

	FarSettingsEnum arg = {sizeof(arg)};
	settings.Enum(FSSF_HISTORY_CMD, arg);

	array<HistoryInfo^>^ result = gcnew array<HistoryInfo^>((int)arg.Count);
	for(int i = 0; i < (int)arg.Count; ++i)
		result[i] = NewHistoryInfo(arg.Histories[i]);

	return result;
}

array<HistoryInfo^>^ History::Editor()
{
	Settings settings(FarGuid);

	FarSettingsEnum arg = {sizeof(arg)};
	settings.Enum(FSSF_HISTORY_EDIT, arg);

	array<HistoryInfo^>^ result = gcnew array<HistoryInfo^>((int)arg.Count);
	for(int i = 0; i < (int)arg.Count; ++i)
		result[i] = NewHistoryInfo(arg.Histories[i]);

	return result;
}

array<HistoryInfo^>^ History::Viewer()
{
	Settings settings(FarGuid);

	FarSettingsEnum arg = {sizeof(arg)};
	settings.Enum(FSSF_HISTORY_VIEW, arg);

	array<HistoryInfo^>^ result = gcnew array<HistoryInfo^>((int)arg.Count);
	for(int i = 0; i < (int)arg.Count; ++i)
		result[i] = NewHistoryInfo(arg.Histories[i]);

	return result;
}

array<HistoryInfo^>^ History::Folder()
{
	Settings settings(FarGuid);

	FarSettingsEnum arg = {sizeof(arg)};
	settings.Enum(FSSF_HISTORY_FOLDER, arg);

	List<HistoryInfo^> list((int)arg.Count);
	for(int i = 0; i < (int)arg.Count; ++i)
	{
		// skip not native folders
		if (memcmp(&arg.Histories[i].PluginId, &FarGuid, sizeof(GUID)))
			continue;
		
		list.Add(NewHistoryInfo(arg.Histories[i]));
	}

	return list.ToArray();
}

array<HistoryInfo^>^ History::Dialog(String^ name)
{
	Settings settings(FarGuid);

	PIN_NE(pin, name);
	int root = settings.OpenSubKey(0, pin);

	FarSettingsEnum arg = {sizeof(arg)};
	settings.Enum(root, arg);

	array<HistoryInfo^>^ result = gcnew array<HistoryInfo^>((int)arg.Count);
	for(int i = 0; i < (int)arg.Count; ++i)
		result[i] = NewHistoryInfo(arg.Histories[i]);

	return result;
}
}
