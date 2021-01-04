
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
#include "History.h"
#include "Settings.h"

namespace FarNet
{
static HistoryInfo^ NewHistoryInfo(const FarSettingsHistory& that)
{
	return gcnew HistoryInfo(gcnew String(that.Name), FileTimeToDateTime(that.Time), that.Lock != 0);
}

array<HistoryInfo^>^ History::GetHistory(GetHistoryArgs^ args)
{
	if (!args)
		throw gcnew ArgumentNullException("args");

	Settings settings(FarGuid);

	// resolve root
	int root;
	if (args->Name)
	{
		// named
		PIN_NE(pin, args->Name);
		root = settings.OpenSubKey(0, pin);
	}
	else
	{
		// fixed
		root = (FARSETTINGS_SUBFOLDERS)args->Kind;
	}

	// get items
	FarSettingsEnum arg = { sizeof(arg) };
	settings.Enum(root, arg);

	// start and end indexes
	int st = 0;
	int en = (int)arg.Count;
	if (args->Last > 0)
	{
		st = en - args->Last;
		if (st < 0)
			st = 0;
	}

	// case: folders, skip plugin records
	if (root == FSSF_HISTORY_FOLDER)
	{
		List<HistoryInfo^> list(en - st);
		for (int i = st; i < en; ++i)
		{
			if (0 == memcmp(&arg.Histories[i].PluginId, &FarGuid, sizeof(GUID)))
				list.Add(NewHistoryInfo(arg.Histories[i]));
		}
		return list.ToArray();
	}

	// case: others, take all records
	auto result = gcnew array<HistoryInfo^>(en - st);
	for (int i = st, r = 0; i < en; ++i, ++r)
	{
		result[r] = NewHistoryInfo(arg.Histories[i]);
	}
	return result;
}
}
