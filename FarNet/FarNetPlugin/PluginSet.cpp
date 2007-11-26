/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

#include "StdAfx.h"
#include "PluginSet.h"
#include "FarImpl.h"

namespace FarManagerImpl
{;
//! Don't use FAR UI
void PluginSet::UnloadPlugins()
{
	for each(BasePlugin^ plug in _plugins)
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

void PluginSet::LoadPlugins()
{
	String^ show = System::Configuration::ConfigurationSettings::AppSettings["FarManager.StartupErrorDialog"];
	if (!String::IsNullOrEmpty(show))
	{
		show = show->Trim();
		_startupErrorDialog = show->Length > 0 && show != "0";
	}

	String^ path = Environment::ExpandEnvironmentVariables(System::Configuration::ConfigurationSettings::AppSettings["FarManager.Plugins"]);
	for each(String^ dir in Directory::GetDirectories(path))
	{
		// skip
		if (Path::GetFileName(dir)->StartsWith("-"))
			continue;

		// load
		LoadPlugin(dir);
	}
}

void PluginSet::LoadPlugin(String^ dir)
{
	try
	{
		// the only *.cfg
		array<String^>^ files = Directory::GetFiles(dir, "*.cfg");
		if (files->Length > 1)
			throw gcnew InvalidOperationException("More than one .cfg files found.");
		if (files->Length == 1)
		{
			LoadConfig(files[0], dir);
			return;
		}

		// DLLs
		LoadAllFrom(dir);
	}
	catch(Exception^ e)
	{
		// WISH: don't use message boxes at this point
		// WISH: make it optional
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

void PluginSet::LoadConfig(String^ file, String^ dir)
{
	for each(String^ line in File::ReadAllLines(file))
	{
		array<String^>^ classes = line->Split(gcnew array<Char>{' '}, StringSplitOptions::RemoveEmptyEntries);
		if (classes->Length == 0)
			continue;
		String^ assemblyName = classes[0];
		Assembly^ assembly = Assembly::LoadFrom(Path::Combine(dir, assemblyName));
		for(int i = 1; i < classes->Length; ++i)
			AddPlugin(assembly->GetType(classes[i], true));
	}
}

void PluginSet::LoadAllFrom(String^ dir)
{
	for each(String^ dll in Directory::GetFiles(dir, "*.dll"))
	{
		Assembly^ assembly = Assembly::LoadFrom(dll);
		for each(Type^ type in assembly->GetExportedTypes())
		{
			if (type->IsAbstract)
				continue;
			if (BasePlugin::typeid->IsAssignableFrom(type))
				AddPlugin(type);
		}
	}
}

void PluginSet::AddPlugin(Type^ type)
{
	BasePlugin^ plugin = (BasePlugin^)Activator::CreateInstance(type);
	_plugins.Add(plugin);
	plugin->Far = Far::Get();

	// case: tool
	ToolPlugin^ tool = dynamic_cast<ToolPlugin^>(plugin);
	if (tool)
	{
		Far::Get()->RegisterTool(plugin, tool->Name, gcnew EventHandler<ToolEventArgs^>(tool, &ToolPlugin::Invoke), tool->Options);
		return;
	}
}

}
