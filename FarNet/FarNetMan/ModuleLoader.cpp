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
	RegistryKey^ keyCache = nullptr;
	try
	{
		// open for writing, to remove obsolete data
		String^ keyCachePath = Far::Net->RegistryPluginsPath + "\\FarNet\\!Cache";
		keyCache = Registry::CurrentUser->OpenSubKey(keyCachePath, true);
		if (!keyCache)
		{
			ToCacheVersion = true;
			return;
		}
		
		// drop the key on version mismatch
		String^ version = keyCache->GetValue(String::Empty, String::Empty)->ToString();
		if (version != CacheVersion.ToString())
		{
			for each(String^ name in keyCache->GetValueNames())
				keyCache->DeleteValue(name);

			ToCacheVersion = true;
			return;
		}
		
		// process cache values
		for each (String^ assemblyPath in keyCache->GetValueNames())
		{
			if (assemblyPath->Length == 0)
				continue;
			
			bool done = false;
			try
			{
				// exists?
				if (!File::Exists(assemblyPath))
					throw gcnew ModuleException;

				// read data
				EnumerableReader reader((array<String^>^)keyCache->GetValue(assemblyPath));

				// Stamp
				String^ assemblyStamp = reader.Read();
				FileInfo fi(assemblyPath);

				// stamp mismatch: do not throw!
				if (assemblyStamp != fi.LastWriteTime.Ticks.ToString(CultureInfo::InvariantCulture))
					continue;
				
				// new manager
				ModuleManager^ manager = gcnew ModuleManager(assemblyPath);

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
				
				List<ProxyCommand^> commands;
				List<ProxyEditor^> editors;
				List<ProxyFiler^> filers;
				List<ProxyTool^> tools;

				for(;;)
				{
					// Type, can be end of data
					String^ itemType = reader.TryRead();
					if (!itemType)
						break;

					// case: host
					if (itemType == "Host")
					{
						String^ className = reader.Read();;
						manager->SetModuleHost(className);
						continue;
					}

					// types:
					if (itemType == "Tool")
					{
						ProxyTool^ it = gcnew ProxyTool(manager, %reader);
						tools.Add(it);
					}
					else if (itemType == "Command")
					{
						ProxyCommand^ it = gcnew ProxyCommand(manager, %reader);
						commands.Add(it);
					}
					else if (itemType == "Editor")
					{
						ProxyEditor^ it = gcnew ProxyEditor(manager, %reader);
						editors.Add(it);
					}
					else if (itemType == "Filer")
					{
						ProxyFiler^ it = gcnew ProxyFiler(manager, %reader);
						filers.Add(it);
					}
					else
					{
						throw gcnew ModuleException;
					}
				}

				// add the name to dictionary and add plugins
				_Managers->Add(manager->ModuleName, manager);
				for each(ProxyCommand^ it in commands)
					Far0::AddModuleCommandInfo(it);
				for each(ProxyEditor^ it in editors)
					Far0::AddModuleEditorInfo(it);
				for each(ProxyFiler^ it in filers)
					Far0::AddModuleFilerInfo(it);
				for each(ProxyTool^ it in tools)
					Far0::AddModuleToolInfo(it);

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
					keyCache->DeleteValue(assemblyPath);
			}
		}
	}
	finally
	{
		if (keyCache)
			keyCache->Close();
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
	
	// add new module info
	ModuleManager^ manager = gcnew ModuleManager(assemblyPath);
	_Managers->Add(assemblyName, manager);

	// load from assembly
	List<ProxyCommand^> commands;
	List<ProxyEditor^> editors;
	List<ProxyFiler^> filers;
	List<ProxyTool^> tools;
	Assembly^ assembly = manager->AssemblyInstance;
	if (classes && classes->Count > 0)
	{
		for each(String^ name in classes)
			AddModuleItem(manager, assembly->GetType(name, true), %commands, %editors, %filers, %tools);
	}
	else
	{
		for each(Type^ type in assembly->GetExportedTypes())
		{
			if (!type->IsAbstract && BaseModuleItem::typeid->IsAssignableFrom(type))
				AddModuleItem(manager, type, %commands, %editors, %filers, %tools);
		}
	}

	// add actions
	for each(ProxyCommand^ it in commands)
		Far0::AddModuleCommandInfo(it);
	for each(ProxyEditor^ it in editors)
		Far0::AddModuleEditorInfo(it);
	for each(ProxyFiler^ it in filers)
		Far0::AddModuleFilerInfo(it);
	for each(ProxyTool^ it in tools)
		Far0::AddModuleToolInfo(it);

	// if the module has no loaded host now then it is cached
	if (!manager->GetLoadedModuleHost())
	{
		if (0 == commands.Count + editors.Count + filers.Count + tools.Count)
			throw gcnew ModuleException("The module should implement at least one action or a preloadable host.");
		
		WriteModuleCache(manager, %commands, %editors, %filers, %tools);
	}
}

// #6 Adds a module item
void ModuleLoader::AddModuleItem(ModuleManager^ manager, Type^ type, List<ProxyCommand^>^ commands, List<ProxyEditor^>^ editors, List<ProxyFiler^>^ filers, List<ProxyTool^>^ tools)
{
	LOG_AUTO(3, "Load module item " + type);

	// host
	if (ModuleHost::typeid->IsAssignableFrom(type))
		manager->SetModuleHost(type);
	// command
	else if (ModuleCommand::typeid->IsAssignableFrom(type))
		commands->Add(gcnew ProxyCommand(manager, type));
	// editor
	else if (ModuleEditor::typeid->IsAssignableFrom(type))
		editors->Add(gcnew ProxyEditor(manager, type));
	// filer
	else if (ModuleFiler::typeid->IsAssignableFrom(type))
		filers->Add(gcnew ProxyFiler(manager, type));
	// tool
	else if (ModuleTool::typeid->IsAssignableFrom(type))
		tools->Add(gcnew ProxyTool(manager, type));
	else
		throw gcnew ModuleException("Unknown module item class type.");
}

//! Don't use Far UI
//! It is already disconnected
void ModuleLoader::RemoveModuleManager(ModuleManager^ manager)
{
	// remove the module
	_Managers->Remove(manager->ModuleName);

	// 1) find all its actions
	List<ProxyAction^> actions;
	for each(ProxyAction^ action in _Actions.Values)
		if (action->Manager == manager)
			actions.Add(action);

	// 2) remove found actions
	for each(ProxyAction^ action in actions)
		action->Unregister();
}

//! Don't use Far UI
void ModuleLoader::UnloadModules()
{
	// unregister managers
	while(_Managers->Count)
		_Managers->Values[0]->Unregister();

	// actions must be removed, too
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

void ModuleLoader::WriteModuleCache(ModuleManager^ manager, List<ProxyCommand^>^ commands, List<ProxyEditor^>^ editors, List<ProxyFiler^>^ filers, List<ProxyTool^>^ tools)
{
	RegistryKey^ keyCache = nullptr;
	try
	{
		keyCache = Registry::CurrentUser->CreateSubKey(Far::Net->RegistryPluginsPath + "\\FarNet\\!Cache");

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

		for each(ProxyTool^ it in tools)
			it->WriteCache(%data);

		for each(ProxyCommand^ it in commands)
			it->WriteCache(%data);

		for each(ProxyEditor^ it in editors)
			it->WriteCache(%data);

		for each(ProxyFiler^ it in filers)
			it->WriteCache(%data);

		array<String^>^ data2 = gcnew array<String^>(data.Count);
		data.CopyTo(data2);
		keyCache->SetValue(manager->AssemblyPath, data2);
	}
	finally
	{
		if (keyCache)
			keyCache->Close();
	}
}

}
