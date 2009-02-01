/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "KeyMacroHost.h"
#include "Far.h"

namespace FarNet
{;
static bool ToBool(Object^ value)
{
	return value && ((int)value) != 0;
}

static String^ GetThreeState(Object^ value1, Object^ value2)
{
	if (value1)
		return ((int)value1) ? "1" : "0";
	if (value2)
		return ((int)value2) ? "0" : "1";
	return String::Empty;
}

array<String^>^ KeyMacroHost::GetNames(String^ area)
{
	if (!area) throw gcnew ArgumentNullException("area");

	String^ path = Far::Instance->RootFar + "\\KeyMacros\\" + area;
	RegistryKey^ key = Registry::CurrentUser->OpenSubKey(path);

	try
	{
		return key ? key->GetSubKeyNames() : gcnew array<String^>(0);
	}
	finally
	{
		key->Close();
	}
}

KeyMacroData^ KeyMacroHost::GetData(String^ area, String^ name)
{
	if (!area) throw gcnew ArgumentNullException("area");
	if (!name) throw gcnew ArgumentNullException("name");

	String^ path = Far::Instance->RootFar + "\\KeyMacros\\" + area + "\\" + name;
	RegistryKey^ key = Registry::CurrentUser->OpenSubKey(path);
	if (!key)
		return nullptr;

	try
	{
		KeyMacroData^ r = gcnew KeyMacroData;
		
		// sequence
		Object^ value = key->GetValue("Sequence");
		if (value)
		{
			array<String^>^ lines = dynamic_cast<array<String^>^>(value);
			if (lines)
			{
				StringBuilder sb;
				int i;
				for(i = 0; i < lines->Length - 1; ++i)
					sb.AppendLine(lines[i]);
				sb.Append(lines[i]);
				r->Sequence = sb.ToString();
			}
			else
			{
				r->Sequence = value->ToString();
			}
		}
		
		// others
		r->Description = key->GetValue("Description", String::Empty)->ToString();
		r->EnableOutput = !ToBool(key->GetValue("DisableOutput", 1));
		r->DisablePlugins = ToBool(key->GetValue("NoSendKeysToPlugins"));
		r->RunAfterFarStart = ToBool(key->GetValue("RunAfterFarStart"));
		r->CommandLine = GetThreeState(key->GetValue("NotEmptyCommandLine"), key->GetValue("EmptyCommandLine"));
		r->SelectedText = GetThreeState(key->GetValue("EVSelection"), key->GetValue("NoEVSelection"));
		r->SelectedItems = GetThreeState(key->GetValue("Selection"), key->GetValue("NoSelection"));
		r->PanelIsPlugin = GetThreeState(key->GetValue("NoFilePanels"), key->GetValue("NoPluginPanels"));
		r->ItemIsDirectory = GetThreeState(key->GetValue("NoFiles"), key->GetValue("NoFolders"));
		r->SelectedItems2 = GetThreeState(key->GetValue("PSelection"), key->GetValue("NoPSelection"));
		r->PanelIsPlugin2 = GetThreeState(key->GetValue("NoFilePPanels"), key->GetValue("NoPluginPPanels"));
		r->ItemIsDirectory2 = GetThreeState(key->GetValue("NoPFiles"), key->GetValue("NoPFolders"));
		return r;
	}
	finally
	{
		key->Close();
	}
}

void KeyMacroHost::Install(String^ area, String^ name, KeyMacroData^ data)
{
	if (!area) throw gcnew ArgumentNullException("area");
	if (!name) throw gcnew ArgumentNullException("name");
	if (!data) throw gcnew ArgumentNullException("data");

	Remove(area, name);

	String^ path = Far::Instance->RootFar + "\\KeyMacros\\" + area + "\\" + name;
	RegistryKey^ key = Registry::CurrentUser->CreateSubKey(path);

	try
	{
		// sequence
		array<String^>^ lines = Regex::Split(data->Sequence, "\\r\\n|\\r|\\n");
		if (lines->Length == 1)
			key->SetValue("Sequence", lines[0]);
		else
			key->SetValue("Sequence", lines);

		// others
		key->SetValue("Description", data->Description);
		key->SetValue("DisableOutput", data->EnableOutput ? 0 : 1);
		if (data->DisablePlugins)
			key->SetValue("NoSendKeysToPlugins", 1);
		if (data->RunAfterFarStart)
			key->SetValue("RunAfterFarStart", 1);
		if (data->CommandLine->Length)
			key->SetValue((data->CommandLine == "1" ? "NotEmptyCommandLine" : "EmptyCommandLine"), 1);
		if (data->SelectedText->Length)
			key->SetValue((data->SelectedText == "1" ? "EVSelection" : "NoEVSelection"), 1);
		if (data->SelectedItems->Length)
			key->SetValue((data->SelectedItems == "1" ? "Selection" : "NoSelection"), 1);
		if (data->PanelIsPlugin->Length)
			key->SetValue((data->PanelIsPlugin == "1" ? "NoFilePanels" : "NoPluginPanels"), 1);
		if (data->ItemIsDirectory->Length)
			key->SetValue((data->ItemIsDirectory == "1" ? "NoFiles" : "NoFolders"), 1);
		if (data->SelectedItems2->Length)
			key->SetValue((data->SelectedItems2 == "1" ? "PSelection" : "NoPSelection"), 1);
		if (data->PanelIsPlugin2->Length)
			key->SetValue((data->PanelIsPlugin2 == "1" ? "NoFilePPanels" : "NoPluginPPanels"), 1);
		if (data->ItemIsDirectory2->Length)
			key->SetValue((data->ItemIsDirectory2 == "1" ? "NoPFiles" : "NoPFolders"), 1);
	}
	finally
	{
		key->Close();
	}
}

void KeyMacroHost::Remove(String^ area, String^ name)
{
	if (!area) throw gcnew ArgumentNullException("area");
	if (!name) throw gcnew ArgumentNullException("name");

	String^ path = Far::Instance->RootFar + "\\KeyMacros\\" + area;
	RegistryKey^ key = Registry::CurrentUser->OpenSubKey(path, true);
	if (!key)
		return;

	try
	{
		key->DeleteSubKey(name, false);
	}
	finally
	{
		key->Close();
	}
}

void KeyMacroHost::Load()
{
	ActlKeyMacro command;
	command.Command = MCMD_LOADALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void KeyMacroHost::Save()
{
	ActlKeyMacro command;
	command.Command = MCMD_SAVEALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void KeyMacroHost::Post(String^ macro)
{
	Post(macro, false, false);
}

void KeyMacroHost::Post(String^ macro, bool enableOutput, bool disablePlugins)
{
	if (!macro) throw gcnew ArgumentNullException("macro");

	CBox sMacro(macro);
	ActlKeyMacro command;
	command.Command = MCMD_POSTMACROSTRING;
	command.Param.PlainText.SequenceText = sMacro;
	command.Param.PlainText.Flags = 0;
	if (!enableOutput)
		command.Param.PlainText.Flags |= KSFLAGS_DISABLEOUTPUT;
	if (disablePlugins)
		command.Param.PlainText.Flags |= KSFLAGS_NOSENDKEYSTOPLUGINS;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void KeyMacroHost::Install(array<System::Collections::IDictionary^>^ dataSet)
{
	Save();
	
	int done = 0;
	try
	{
		KeyMacroData^ data = gcnew KeyMacroData;
		String^ area;
		String^ name;

		Dictionary<String^, bool> paths;
		for each(System::Collections::IDictionary^ map in dataSet)
		{
			// reset
			if (!map)
			{
				data = gcnew KeyMacroData;
				area = nullptr;
				name = nullptr;
				continue;
			}

			// get data
			for each(String^ key in map->Keys)
			{
				Object^ value = map[key];
				if (EqualsOrdinal(key, "Area"))
				{
					area = value->ToString();
				}
				else if (EqualsOrdinal(key, "Name"))
				{
					name = value->ToString();
				}
				else
				{
					data->GetType()->InvokeMember(
						key,
						BindingFlags::SetProperty | BindingFlags::Public | BindingFlags::Instance | BindingFlags::IgnoreCase,
						nullptr, data, gcnew array<Object^> { value }, CultureInfo::InvariantCulture);
				}
			}

			// not ready?
			if (ES(area) || ES(name) || data->Sequence->Length == 0)
				continue;

			// dupe?
			String^ path = area + "\\" + name;
			if (paths.ContainsKey(path))
				throw gcnew ArgumentException("Macros '" + path + "' is defined twice.");
			paths.Add(path, 0);

			// install
			Install(area, name, data);
			++done;
		}
	}
	finally
	{
		Load();
	}

	if (done == 0)
		throw gcnew ArgumentException("No macro is defined. Ensure 'Area', 'Name' and 'Sequence'.");
}
}
