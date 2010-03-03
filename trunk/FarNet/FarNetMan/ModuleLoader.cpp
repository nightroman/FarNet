/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "ModuleLoader.h"
#include "Far0.h"
#include "ModuleManager.h"
#include "ModuleProxy.h"

// The cache version
const int CacheVersion = 8;
static bool ToCacheVersion;

namespace FarNet
{;
// #1 Load all
void ModuleLoader::LoadModules()
{
	// read modules from the cache, up-to-date modules get loaded with static info
	ReadModuleCache();

	// read from module directories:
	String^ path = Environment::ExpandEnvironmentVariables(ConfigurationManager::AppSettings["FarNet.Modules"]);
	for each(String^ dir in Directory::GetDirectories(path))
	{
		// skip
		if (Path::GetFileName(dir)->StartsWith("-"))
			continue;

		// load
		LoadFromDirectory(dir);
	}
}

// #2 Read cache
void ModuleLoader::ReadModuleCache()
{
	LOG_AUTO(3, "Read module cache");

	IRegistryKey^ keyCache = nullptr;
	try
	{
		// open for writing, to remove obsolete data
		keyCache = Far::Net->OpenRegistryKey("Plugins\\FarNet\\!Cache", true);
		
		// different version: drop cache values
		String^ version = keyCache->GetValue(String::Empty, String::Empty)->ToString();
		if (version != CacheVersion.ToString())
		{
			for each(String^ name in keyCache->GetValueNames())
				keyCache->SetValue(name, nullptr);

			ToCacheVersion = true;
			return;
		}
		
		// process cache values
		for each (String^ assemblyPath in keyCache->GetValueNames())
		{
			if (assemblyPath->Length == 0)
				continue;
			
			bool done = false;
			ModuleManager^ manager = nullptr;
			try
			{
				// exists?
				if (!File::Exists(assemblyPath))
					throw gcnew ModuleException;

				// read data
				EnumerableReader reader((array<String^>^)keyCache->GetValue(assemblyPath, nullptr));

				// Stamp
				String^ assemblyStamp = reader.Read();
				FileInfo fi(assemblyPath);

				// stamp mismatch: do not throw!
				if (assemblyStamp != fi.LastWriteTime.Ticks.ToString(CultureInfo::InvariantCulture))
					continue;
				
				// new manager, add it now, remove later on errors
				manager = gcnew ModuleManager(assemblyPath);
				_Managers->Add(manager->ModuleName, manager);

				// culture of cached resources
				String^ savedCulture = reader.Read();
				
				// check the culture
				if (savedCulture->Length)
				{
					// the culture changed, ignore the cache
					if (savedCulture != manager->CurrentUICulture->Name)
						continue;

					// restore the flag
					manager->CachedResources = true;
				}
				
				for(;;)
				{
					// Kind, can be end of data
					String^ kindText = reader.TryRead();
					if (!kindText)
						break;
					
					ModuleItemKind kind = (ModuleItemKind)Enum::Parse(ModuleItemKind::typeid, kindText);
					switch(kind)
					{
					case ModuleItemKind::Host:
						manager->SetModuleHost(reader.Read());
						break;
					case ModuleItemKind::Command:
						Far0::RegisterProxyCommand(gcnew ProxyCommand(manager, %reader));
						break;
					case ModuleItemKind::Editor:
						Far0::RegisterProxyEditor(gcnew ProxyEditor(manager, %reader));
						break;
					case ModuleItemKind::Filer:
						Far0::RegisterProxyFiler(gcnew ProxyFiler(manager, %reader));
						break;
					case ModuleItemKind::Tool:
						Far0::RegisterProxyTool(gcnew ProxyTool(manager, %reader));
						break;
					default:
						throw gcnew ModuleException;
					}
				}

				done = true;
			}
			catch(ModuleException^)
			{
				// ignore known issues
			}
			catch(Exception^ ex)
			{
				throw gcnew ModuleException(
					"Error on reading the registry cache 'Plugins\\FarNet\\!Cache'.", ex);
			}
			finally
			{
				if (!done)
				{
					keyCache->SetValue(assemblyPath, nullptr);
					if (manager)
						RemoveModuleManager(manager);
				}
			}
		}
	}
	finally
	{
		delete keyCache;
	}
}

// #3
void ModuleLoader::LoadFromDirectory(String^ dir)
{
	try
	{
		// the manifest
		array<String^>^ manifests = Directory::GetFiles(dir, "*.cfg");
		if (manifests->Length == 1)
		{
			LoadFromManifest(manifests[0], dir);
			return;
		}
		if (manifests->Length > 1)
			throw gcnew ModuleException("More than one .cfg files found.");

		// load the only assembly
		array<String^>^ assemblies = Directory::GetFiles(dir, "*.dll");
		if (assemblies->Length == 1)
			LoadFromAssembly(assemblies[0], nullptr);
		else if (assemblies->Length > 1)
			throw gcnew ModuleException("More than one .dll files found. Expected exactly one .dll file or exactly one .cfg file.");

		//! If the folder has no .dll or .cfg files (not yet built sources) then just ignore
	}
	catch(Exception^ e)
	{
		// Wish: no UI on loading
		String^ msg = "ERROR: module " + dir + ":\n" + Log::FormatException(e) + "\n" + e->StackTrace;
		Far::Net->Write(msg, ConsoleColor::Red);
		Log::TraceError(msg);
	}
}

// #4
void ModuleLoader::LoadFromManifest(String^ file, String^ dir)
{
	array<String^>^ lines = File::ReadAllLines(file);
	if (lines->Length == 0)
		throw gcnew ModuleException("The manifest file is empty.");
	
	// assembly
	String^ path = lines[0]->TrimEnd();
	if (path->Length == 0)
		throw gcnew ModuleException("Expected the module assembly name as the first line of the manifest file.");
	path = Path::Combine(dir, path);
	
	// collect classes
	List<String^> classes(lines->Length - 1);
	for(int i = 1; i < lines->Length; ++i)
	{
		String^ name = lines[i]->Trim();
		if (name->Length)
			classes.Add(name);
	}
	
	// load with classes, if any
	LoadFromAssembly(path, %classes);
}

// #5 Loads the assembly, writes cache
void ModuleLoader::LoadFromAssembly(String^ assemblyPath, List<String^>^ classes)
{
	// the name
	String^ assemblyName = Path::GetFileName(assemblyPath);

	// already loaded (normally from cache)?
	if (_Managers->ContainsKey(assemblyName))
		return;
	
	// add new module manager now, it will be removed on errors
	ModuleManager^ manager = gcnew ModuleManager(assemblyPath);
	_Managers->Add(assemblyName, manager);
	bool done = false;
	try
	{
		LOG_AUTO(3, String::Format("Load module {0}", manager->ModuleName));

		int actionCount = 0;
		Assembly^ assembly = manager->AssemblyInstance;
		if (classes && classes->Count > 0)
		{
			for each(String^ name in classes)
				actionCount += LoadClass(manager, assembly->GetType(name, true));
		}
		else
		{
			for each(Type^ type in assembly->GetExportedTypes())
			{
				if (!type->IsAbstract && BaseModuleItem::typeid->IsAssignableFrom(type))
					actionCount += LoadClass(manager, type);
			}
		}

		// if the module has the host to load then load it now, if it is not loaded then the module should be cached
		if (!manager->LoadLoadableModuleHost())
		{
			if (0 == actionCount)
				throw gcnew ModuleException("The module must implement a public action or a preloadable host.");

			WriteModuleCache(manager);
		}

		// done
		done = true;
	}
	finally
	{
		if (!done)
			RemoveModuleManager(manager);
	}
}

// #6 Adds a module item
int ModuleLoader::LoadClass(ModuleManager^ manager, Type^ type)
{
	LOG_AUTO(3, "Load class " + type);

	// case: host
	if (ModuleHost::typeid->IsAssignableFrom(type))
	{
		manager->SetModuleHost(type);
		return 0;
	}
	
	// command
	if (ModuleCommand::typeid->IsAssignableFrom(type))
		Far0::RegisterProxyCommand(gcnew ProxyCommand(manager, type));
	// editor
	else if (ModuleEditor::typeid->IsAssignableFrom(type))
		Far0::RegisterProxyEditor(gcnew ProxyEditor(manager, type));
	// filer
	else if (ModuleFiler::typeid->IsAssignableFrom(type))
		Far0::RegisterProxyFiler(gcnew ProxyFiler(manager, type));
	// tool
	else if (ModuleTool::typeid->IsAssignableFrom(type))
		Far0::RegisterProxyTool(gcnew ProxyTool(manager, type));
	else
		throw gcnew ModuleException("Unknown module class type.");
	return 1;
}

//! Don't use Far UI
//! It is already disconnected
void ModuleLoader::RemoveModuleManager(ModuleManager^ manager)
{
	// remove the module
	_Managers->Remove(manager->ModuleName);

	// 1) gather its actions
	List<ProxyAction^> actions;
	for each(ProxyAction^ action in _Actions.Values)
		if (action->Manager == manager)
			actions.Add(action);

	// 2) unregister its actions
	for each(ProxyAction^ action in actions)
		action->Unregister();
}

//! Don't use Far UI
void ModuleLoader::UnloadModules()
{
	// unregister managers
	while(_Managers->Count)
		_Managers->Values[0]->Unregister();

	// actions are removed
	assert(_Actions.Count == 0);
}

bool ModuleLoader::CanExit()
{
	for each(ModuleManager^ manager in _Managers->Values)
	{
		if (manager->GetLoadedModuleHost() && !manager->GetLoadedModuleHost()->CanExit())
			return false;
	}

	return true;
}

void ModuleLoader::WriteModuleCache(ModuleManager^ manager)
{
	IRegistryKey^ keyCache = nullptr;
	try
	{
		keyCache = Far::Net->OpenRegistryKey("Plugins\\FarNet\\!Cache", true);

		// update cache version
		if (ToCacheVersion)
		{
			ToCacheVersion = false;
			keyCache->SetValue(String::Empty, CacheVersion.ToString());
		}

		FileInfo fi(manager->AssemblyPath);
		List<String^> data;

		// Stamp
		data.Add(fi.LastWriteTime.Ticks.ToString(CultureInfo::InvariantCulture));

		// Culture
		if (manager->CachedResources)
			data.Add(manager->CurrentUICulture->Name);
		else
			data.Add(String::Empty);

		// host
		String^ hostClassName = manager->GetModuleHostClassName();
		if (hostClassName)
		{
			// Type
			data.Add("Host");
			// Class
			data.Add(hostClassName);
		}

		// write actions of the manager
		for each(ProxyAction^ it in _Actions.Values)
			if (it->Manager == manager)
				it->WriteCache(%data);

		// write to the registry
		keyCache->SetValue(manager->AssemblyPath, data.ToArray());
	}
	finally
	{
		delete keyCache;
	}
}

array<ProxyTool^>^ ModuleLoader::GetTools(ModuleToolOptions option)
{
	List<ProxyTool^> list(_Actions.Count);
	for each(ProxyAction^ action in _Actions.Values)
	{
		if (action->Kind != ModuleItemKind::Tool)
			continue;
		
		ProxyTool^ tool = (ProxyTool^)action;
		if (int(tool->Options & option))
			list.Add(tool);
	}
	return list.ToArray();
}

List<ProxyTool^>^ ModuleLoader::GetTools()
{
	List<ProxyTool^>^ result = gcnew List<ProxyTool^>(_Actions.Count);
	for each(ProxyAction^ action in _Actions.Values)
	{
		if (action->Kind == ModuleItemKind::Tool)
			result->Add((ProxyTool^)action);
	}
	return result;
}

}
