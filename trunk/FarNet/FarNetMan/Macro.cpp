/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Macro.h"
#include "Far.h"

namespace FarNet
{;
IMacro^ Far::Macro::get()
{
	return gcnew Macro0;
}

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

array<String^>^ Macro0::GetNames(String^ area)
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

Macro^ Macro0::GetMacro(String^ area, String^ name)
{
	if (!area) throw gcnew ArgumentNullException("area");
	if (!name) throw gcnew ArgumentNullException("name");

	String^ path = Far::Instance->RootFar + "\\KeyMacros\\" + area + "\\" + name;
	RegistryKey^ key = Registry::CurrentUser->OpenSubKey(path);
	if (!key)
		return nullptr;

	try
	{
		Macro^ r = gcnew Macro;
		r->Area = area;
		r->Name = name;
		
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

void Macro0::Remove(String^ area, String^ name)
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

void Macro0::Load()
{
	ActlKeyMacro command;
	command.Command = MCMD_LOADALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void Macro0::Save()
{
	ActlKeyMacro command;
	command.Command = MCMD_SAVEALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &command))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void Macro0::Install(Macro^ macro)
{
	if (!macro) throw gcnew ArgumentNullException("macro");
	if (ES(macro->Area)) throw gcnew ArgumentException("macro.Area cannot be empty.");
	if (ES(macro->Name)) throw gcnew ArgumentException("macro.Name cannot be empty.");

	Remove(macro->Area, macro->Name);

	String^ path = Far::Instance->RootFar + "\\KeyMacros\\" + macro->Area + "\\" + macro->Name;
	RegistryKey^ key = Registry::CurrentUser->CreateSubKey(path);

	try
	{
		// sequence
		array<String^>^ lines = Regex::Split(macro->Sequence, "\\r\\n|\\r|\\n");
		if (lines->Length == 1)
			key->SetValue("Sequence", lines[0]);
		else
			key->SetValue("Sequence", lines);

		// others
		key->SetValue("Description", macro->Description);
		key->SetValue("DisableOutput", macro->EnableOutput ? 0 : 1);
		if (macro->DisablePlugins)
			key->SetValue("NoSendKeysToPlugins", 1);
		if (macro->RunAfterFarStart)
			key->SetValue("RunAfterFarStart", 1);
		if (macro->CommandLine->Length)
			key->SetValue((macro->CommandLine == "1" ? "NotEmptyCommandLine" : "EmptyCommandLine"), 1);
		if (macro->SelectedText->Length)
			key->SetValue((macro->SelectedText == "1" ? "EVSelection" : "NoEVSelection"), 1);
		if (macro->SelectedItems->Length)
			key->SetValue((macro->SelectedItems == "1" ? "Selection" : "NoSelection"), 1);
		if (macro->PanelIsPlugin->Length)
			key->SetValue((macro->PanelIsPlugin == "1" ? "NoFilePanels" : "NoPluginPanels"), 1);
		if (macro->ItemIsDirectory->Length)
			key->SetValue((macro->ItemIsDirectory == "1" ? "NoFiles" : "NoFolders"), 1);
		if (macro->SelectedItems2->Length)
			key->SetValue((macro->SelectedItems2 == "1" ? "PSelection" : "NoPSelection"), 1);
		if (macro->PanelIsPlugin2->Length)
			key->SetValue((macro->PanelIsPlugin2 == "1" ? "NoFilePPanels" : "NoPluginPPanels"), 1);
		if (macro->ItemIsDirectory2->Length)
			key->SetValue((macro->ItemIsDirectory2 == "1" ? "NoPFiles" : "NoPFolders"), 1);
	}
	finally
	{
		key->Close();
	}
}

void Macro0::Install(array<Macro^>^ macros)
{
	Save();

	if (!macros)
		return;

	List<String^> done;
	try
	{
		for each(Macro^ macro in macros)
		{
			String^ path1 = String::Format("{0}\\{1}", macro->Area, macro->Name);
			String^ path2 = path1->ToUpperInvariant();
			if (done.IndexOf(path2) >= 0)
				throw gcnew InvalidOperationException(String::Format("Macro '{0}' is defined twice.", path1));

			done.Add(path2);
			Install(macro);
		}
	}
	finally
	{
		Load();
	}
}

}
