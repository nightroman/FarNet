/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "PluginManager.h"
#include "FarImpl.h"

namespace FarManagerImpl
{;
PluginManager::PluginManager(Far^ plugin) : _far(plugin)
{
}

void PluginManager::LoadPlugins()
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

void PluginManager::UnloadPlugins()
{
	for each(IPlugin^ plug in _plugins)
		plug->Far = nullptr;
	_plugins.Clear();
	_far = nullptr;
}

void PluginManager::AddPlugin(Type^ type)
{
	Trace::WriteLine("Class:" + type->Name);
	IPlugin^ plugin = (IPlugin^)Activator::CreateInstance(type);
	_plugins.Add(plugin);
	plugin->Far = _far;
	Trace::WriteLine("Attached:" + type->Name);
}

void PluginManager::LoadConfig(StreamReader^ text, String^ dir)
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

void PluginManager::LoadAllFrom(String^ dir)
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

void PluginManager::LoadPlugin(String^ dir)
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
			_far->ShowError("ERROR in plugin " + dir, e);
		}
		else
		{
			_far->Write(
				"ERROR: plugin " + dir + ":\n" + ExceptionInfo(e, true),
				ConsoleColor::Red, Console::BackgroundColor);
		}
	}
}
}
