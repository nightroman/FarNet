/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Macro.h"

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

void Macro0::Load()
{
	ActlKeyMacro args;
	args.Command = MCMD_LOADALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &args))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

void Macro0::Save()
{
	ActlKeyMacro args;
	args.Command = MCMD_SAVEALL;
	if (!Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &args))
		throw gcnew OperationCanceledException(__FUNCTION__ " failed.");
}

array<String^>^ Macro0::GetNames(MacroArea area)
{
	String^ path = Far::Net->RegistryFarPath + "\\KeyMacros\\" + (area == MacroArea::Root ? "" : area.ToString());
	RegistryKey^ key = Registry::CurrentUser->OpenSubKey(path);
	if (!key)
		return gcnew array<String^>(0);

	try
	{
		if (area == MacroArea::Consts || area == MacroArea::Vars)
			return key->GetValueNames();
		else
			return key->GetSubKeyNames();
	}
	finally
	{
		key->Close();
	}
}

Object^ Macro0::GetScalar(MacroArea area, String^ name)
{
	String^ path = Far::Net->RegistryFarPath + "\\KeyMacros\\" + area.ToString();
	RegistryKey^ key = Registry::CurrentUser->OpenSubKey(path);
	if (!key)
		return nullptr;

	try
	{
		return RegistryValueToObject(key->GetValue(name));
	}
	finally
	{
		key->Close();
	}
}

Object^ Macro0::GetConstant(String^ name)
{
	return GetScalar(MacroArea::Consts, name);
}

Object^ Macro0::GetVariable(String^ name)
{
	return GetScalar(MacroArea::Vars, name);
}

Macro^ Macro0::GetMacro(MacroArea area, String^ name)
{
	if (!name) throw gcnew ArgumentNullException("name");
	
	// _100211_140534 FarMacro workaround
	name = name->Replace("(Slash)", "/");

	String^ path = Far::Net->RegistryFarPath + "\\KeyMacros\\" + area.ToString() + "\\" + name;
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
			r->Sequence = RegistryValueToObject(value)->ToString();
		
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

void Macro0::Remove(MacroArea area, String^ name)
{
	String^ path = Far::Net->RegistryFarPath + "\\KeyMacros\\" + area.ToString();
	RegistryKey^ key = Registry::CurrentUser->OpenSubKey(path, true);
	if (!key)
		return;

	try
	{
		if (area == MacroArea::Consts || area == MacroArea::Vars)
		{
			// do not throw on missing value
			key->DeleteValue(name, false);
		}
		else
		{
			// _100211_140534 FarMacro workaround
			name = name->Replace("(Slash)", "/");

			key->DeleteSubKey(name, false);
		}
	}
	finally
	{
		key->Close();
	}
}

void Macro0::Remove(MacroArea area, array<String^>^ names)
{
	if (area == MacroArea::Root)
		throw gcnew ArgumentException("Invalid 'area'.");

	if (!ManualSaveLoad)
		Save();

	if (!names || names->Length == 0 || names->Length == 1 && ES(names[0]))
	{
		Remove(area, String::Empty);
		return;
	}

	try
	{
		for each(String^ name in names)
			if (SS(name))
				Remove(area, name);
	}
	finally
	{
		if (!ManualSaveLoad)
			Load();
	}
}

// Converts the string array to the text; returns other objects not changed.
Object^ Macro0::RegistryValueToObject(Object^ value)
{
	array<String^>^ lines = dynamic_cast<array<String^>^>(value);
	if (!lines)
		return value;
	
	StringBuilder sb;
	int i;
	for(i = 0; i < lines->Length - 1; ++i)
		sb.AppendLine(lines[i]);
	sb.Append(lines[i]);
	return sb.ToString();
}

// Converts the text to the string array or the same string.
Object^ Macro0::StringToRegistryValue(String^ text)
{
	array<String^>^ lines = Regex::Split(text, "\\r\\n|\\r|\\n");
	if (lines->Length == 1)
		return text;
	else
		return lines;
}

void Macro0::InstallScalar(MacroArea area, String^ name, Object^ value)
{
	if (!name)
		throw gcnew ArgumentNullException("name");
	if (!value)
		throw gcnew ArgumentNullException("value");

	String^ path = Far::Net->RegistryFarPath + "\\KeyMacros\\" + area.ToString();
	RegistryKey^ key = Registry::CurrentUser->CreateSubKey(path);

	try
	{
		if (value->GetType() == Int64::typeid)
			key->SetValue(name, value, RegistryValueKind::QWord);
		else
			key->SetValue(name, (dynamic_cast<String^>(value) ? StringToRegistryValue(value->ToString()) : value));

		if (!ManualSaveLoad)
			Load();
	}
	finally
	{
		key->Close();
	}
}

void Macro0::InstallConstant(String^ name, Object^ value)
{
	InstallScalar(MacroArea::Consts, name, value);
}

void Macro0::InstallVariable(String^ name, Object^ value)
{
	InstallScalar(MacroArea::Vars, name, value);
}

void Macro0::Install(Macro^ macro)
{
	if (!macro) throw gcnew ArgumentNullException("macro");

	String^ macroName = macro->Name;
#define Name use_macroName
	if (SS(macroName))
	{
		// _100211_140534 Take into account FarMacro workaround
		macroName = macroName->Replace("(Slash)", "/");
		
		// remove the old
		Remove(macro->Area, macroName);
	}

	String^ path = Far::Net->RegistryFarPath + "\\KeyMacros\\" + macro->Area.ToString() + "\\" + macroName;
	RegistryKey^ key = Registry::CurrentUser->CreateSubKey(path);

	if (ES(macroName))
		return;

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
#undef Name
}

void Macro0::Install(array<Macro^>^ macros)
{
	if (!macros)
		return;

	if (!ManualSaveLoad)
		Save();

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
		if (!ManualSaveLoad)
			Load();
	}
}

MacroParseError^ Macro0::Check(String^ sequence, bool silent)
{
	PIN_ES(pin, sequence);
	
	ActlKeyMacro args;
	args.Command = MCMD_CHECKMACRO;
	args.Param.PlainText.SequenceText = pin;
	args.Param.PlainText.Flags = silent ? KSFLAGS_SILENTCHECK : 0;

	//! it always gets ErrCode
	Info.AdvControl(Info.ModuleNumber, ACTL_KEYMACRO, &args);
	if (args.Param.MacroResult.ErrCode == MPEC_SUCCESS)
		return nullptr;
	
	MacroParseError^ r = gcnew MacroParseError;
	r->ErrorCode = (MacroParseStatus)args.Param.MacroResult.ErrCode;
	r->Token = gcnew String(args.Param.MacroResult.ErrSrc);
	r->Line = args.Param.MacroResult.ErrPos.Y;
	r->Pos = args.Param.MacroResult.ErrPos.X;
	return r;
}

}
