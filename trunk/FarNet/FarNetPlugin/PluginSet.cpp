/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "PluginSet.h"
#include "FarImpl.h"

namespace FarManagerImpl
{;
void PluginSet::LoadPlugins()
{
	Trace::WriteLine("Loading plugins");

	String^ show = System::Configuration::ConfigurationSettings::AppSettings["FarManager.StartupErrorDialog"];
	if (!String::IsNullOrEmpty(show))
	{
		show = show->Trim();
		_startupErrorDialog = show->Length > 0 && show != "0";
	}

	String^ path = Environment::ExpandEnvironmentVariables(System::Configuration::ConfigurationSettings::AppSettings["FarManager.Plugins"]);
	for each(DirectoryInfo^ dir in DirectoryInfo(path).GetDirectories())
		LoadPlugin(dir->FullName);
}

void PluginSet::UnloadPlugins()
{
	for each(IPlugin^ plug in _plugins)
	{
		try
		{
			plug->Far = nullptr;
		}
		catch(Exception^ e)
		{
			Console::WriteLine();
			Console::ForegroundColor = ConsoleColor::Red;
			Console::WriteLine(plug->ToString() + " error:");
			Console::WriteLine(e->Message);
			if (_startupErrorDialog)
				Console::ReadKey(true);
			else
				System::Threading::Thread::Sleep(1000);
		}
	}
	_plugins.Clear();
}

void PluginSet::AddPlugin(Type^ type)
{
	Trace::WriteLine("Class:" + type->Name);
	IPlugin^ plugin = (IPlugin^)Activator::CreateInstance(type);
	_plugins.Add(plugin);
	plugin->Far = Far::Get();
	Trace::WriteLine("Attached:" + type->Name);
}

void PluginSet::LoadConfig(StreamReader^ text, String^ dir)
{
	try
	{
		String^ dirBin = dir + "\\Bin";
		String^ line;
		while ((line = text->ReadLine()) != nullptr)
		{
			Trace::WriteLine("Loaded Line:" + line);
			array<String^>^ classes = line->Split(' ');
			String^ assemblyName = classes[0];
			Trace::WriteLine("Assembly:" + assemblyName);
			Assembly^ assembly = Assembly::LoadFrom(dirBin + "\\" + assemblyName);
			for(int i = 1; i < classes->Length; ++i)
				AddPlugin(assembly->GetType(classes[i], true));
		}
	}
	finally
	{
		text->Close();
	}
}

void PluginSet::LoadAllFrom(String^ dir)
{
	for each(String^ dll in Directory::GetFiles(dir, "*.dll"))
	{
		Assembly^ assembly = Assembly::LoadFrom(dll);
		for each(Type^ type in assembly->GetExportedTypes())
		{
			if (!type->IsAbstract && IPlugin::typeid->IsAssignableFrom(type))
				AddPlugin(type);
		}
	}
}

void PluginSet::LoadPlugin(String^ dir)
{
	Trace::WriteLine("Plugin:" + dir);
	try
	{
		// folder Cfg
		String^ dirCfg = dir + "\\Cfg";
		if (Directory::Exists(dirCfg))
		{
			String^ cfg = dirCfg + "\\plugin.cfg";
			if (File::Exists(cfg))
			{
				LoadConfig(File::OpenText(cfg), dirCfg);
				return;
			}
		}

		// folder Bin
		String^ dirBin = dir + "\\Bin";
		if (Directory::Exists(dirBin))
		{
			LoadAllFrom(dirBin);
			return;
		}

		// folder itself
		LoadAllFrom(dir);
	}
	catch(Exception^ e)
	{
		// USER REQUEST 1: don't use message boxes at this point
		// USER REQUEST 2: make it optional
		if (_startupErrorDialog)
		{
			Far::Get()->ShowError("ERROR in plugin " + dir, e);
		}
		else
		{
			Far::Get()->Write(
				"ERROR: plugin " + dir + ":\n" + ExceptionInfo(e, true),
				ConsoleColor::Red, Console::BackgroundColor);
		}
	}
}
}
