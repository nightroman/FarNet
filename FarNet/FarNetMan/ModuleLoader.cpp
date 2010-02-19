/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "ModuleLoader.h"
#include "Far0.h"
#include "ModuleItems.h"
#include "ModuleManager.h"

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
		String^ keyCachePath = Far::Net->RegistryPluginsPath + "\\FarNet\\<cache>";
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
				System::Collections::IEnumerator^ data = ((array<String^>^)keyCache->GetValue(assemblyPath))->GetEnumerator();

				// Stamp
				String^ assemblyStamp = NextString(data);
				FileInfo fi(assemblyPath);

				// stamp mismatch: do not throw!
				if (assemblyStamp != fi.LastWriteTime.Ticks.ToString(CultureInfo::InvariantCulture))
					continue;
				
				// new manager
				ModuleManager^ manager = gcnew ModuleManager(assemblyPath);

				// culture of cached resources
				String^ savedCulture = NextString(data);
				
				// check the culture
				if (savedCulture->Length)
				{
					// the culture changed, ignore the cache
					if (savedCulture != manager->CurrentUICulture->Name)
						continue;

					// restore the flag
					manager->CachedResources = true;
				}
				
				List<ModuleCommandInfo^> commands;
				List<ModuleEditorInfo^> editors;
				List<ModuleFilerInfo^> filers;
				List<ModuleToolInfo^> tools;

				for(;;)
				{
					// Type, can be end of data
					if (!data->MoveNext())
						break;
					String^ itemType = data->Current->ToString();

					// case: host
					if (itemType == "Host")
					{
						String^ className = NextString(data);
						manager->SetModuleHost(className);
						continue;
					}

					// types:
					if (itemType == "Tool")
					{
						ModuleToolInfo^ tool = gcnew ModuleToolInfo(manager, data);
						tools.Add(tool);
					}
					else if (itemType == "Command")
					{
						ModuleCommandInfo^ tool = gcnew ModuleCommandInfo(manager, data);
						commands.Add(tool);
					}
					else if (itemType == "Editor")
					{
						ModuleEditorInfo^ tool = gcnew ModuleEditorInfo(manager, data);
						editors.Add(tool);
					}
					else if (itemType == "Filer")
					{
						ModuleFilerInfo^ tool = gcnew ModuleFilerInfo(manager, data);
						filers.Add(tool);
					}
					else
					{
						throw gcnew ModuleException;
					}
				}

				// add the name to dictionary and add plugins
				_ModuleManagers->Add(Path::GetFileName(assemblyPath), manager);
				if (commands.Count)
					Far0::RegisterCommands(%commands);
				if (editors.Count)
					Far0::RegisterEditors(%editors);
				if (filers.Count)
					Far0::RegisterFilers(%filers);
				if (tools.Count)
					Far0::RegisterTools(%tools);

				done = true;
			}
			catch(ModuleException^)
			{
			}
			catch(Exception^ ex)
			{
				throw gcnew ModuleException(
					"Error on reading the cache. Remove registry FarNet\\<cache> manually and restart Far.", ex);
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
	if (_ModuleManagers->ContainsKey(assemblyName))
		return;
	
	// add new module info
	ModuleManager^ manager = gcnew ModuleManager(assemblyPath);
	_ModuleManagers->Add(assemblyName, manager);

	// load from assembly
	List<ModuleCommandInfo^> commands;
	List<ModuleEditorInfo^> editors;
	List<ModuleFilerInfo^> filers;
	List<ModuleToolInfo^> tools;
	Assembly^ assembly = manager->AssemblyInstance;
	if (classes && classes->Count > 0)
	{
		for each(String^ name in classes)
			AddModuleEntry(manager, assembly->GetType(name, true), %commands, %editors, %filers, %tools);
	}
	else
	{
		for each(Type^ type in assembly->GetExportedTypes())
		{
			if (!type->IsAbstract && BaseModuleItem::typeid->IsAssignableFrom(type))
				AddModuleEntry(manager, type, %commands, %editors, %filers, %tools);
		}
	}

	// add tools
	if (commands.Count)
		Far0::RegisterCommands(%commands);
	if (editors.Count)
		Far0::RegisterEditors(%editors);
	if (filers.Count)
		Far0::RegisterFilers(%filers);
	if (tools.Count)
		Far0::RegisterTools(%tools);

	// if the module has no loaded 
	if (!manager->GetLoadedModuleHost())
	{
		if (0 == commands.Count + editors.Count + filers.Count + tools.Count)
			throw gcnew ModuleException("The module should implement at least one tool class or a preloadable host class.");
		
		WriteModuleCache(manager, %commands, %editors, %filers, %tools);
	}
}

// #6 Adds a module entry
void ModuleLoader::AddModuleEntry(ModuleManager^ manager, Type^ type, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools)
{
	LOG_AUTO(3, "Load module entry " + type);

	// host:
	if (ModuleHost::typeid->IsAssignableFrom(type))
	{
		manager->SetModuleHost(type);
	}
	// tool:
	else if (ModuleTool::typeid->IsAssignableFrom(type))
	{
		ModuleToolInfo^ pt = gcnew ModuleToolInfo(manager, type);
		tools->Add(pt);
	}
	// command:
	else if (ModuleCommand::typeid->IsAssignableFrom(type))
	{
		ModuleCommandInfo^ pc = gcnew ModuleCommandInfo(manager, type);
		commands->Add(pc);
	}
	// editor:
	else if (ModuleEditor::typeid->IsAssignableFrom(type))
	{
		ModuleEditorInfo^ pe = gcnew ModuleEditorInfo(manager, type);
		editors->Add(pe);
	}
	// case: filer
	else if (ModuleFiler::typeid->IsAssignableFrom(type))
	{
		ModuleFilerInfo^ pf = gcnew ModuleFilerInfo(manager, type);
		filers->Add(pf);
	}
	else
	{
		throw gcnew ModuleException("Unknown module class type.");
	}
}

//! Don't use Far UI
void ModuleLoader::UnloadModuleItem(BaseModuleItem^ item)
{
	LOG_AUTO(3, "Unload module item " + item);

	// tool:
	BaseModuleTool^ tool = dynamic_cast<BaseModuleTool^>(item);
	if (tool)
		return;

	ModuleHost^ host = (ModuleHost^)item;
	for each(ModuleManager^ manager in _ModuleManagers->Values)
	{
		if (manager->GetLoadedModuleHost() == host)
		{
			manager->Unload();
			break;
		}
	}
}

//! Don't use Far UI
void ModuleLoader::UnloadModules()
{
	for(int i = _ModuleManagers->Count; --i >= 0;)
	{
		ModuleManager^ manager = _ModuleManagers->Values[i];
		manager->Unload();
	}

	_ModuleManagers->Clear();
}

bool ModuleLoader::CanExit()
{
	for each(ModuleManager^ manager in _ModuleManagers->Values)
	{
		if (manager->GetLoadedModuleHost() && !manager->GetLoadedModuleHost()->CanExit())
			return false;
	}

	return true;
}

void ModuleLoader::WriteModuleCache(ModuleManager^ manager, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools)
{
	RegistryKey^ keyCache = nullptr;
	try
	{
		keyCache = Registry::CurrentUser->CreateSubKey(Far::Net->RegistryPluginsPath + "\\FarNet\\<cache>");

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

		for each(ModuleToolInfo^ tool in tools)
		{
			data.Add("Tool");
			tool->WriteCache(%data);
		}

		for each(ModuleCommandInfo^ tool in commands)
		{
			data.Add("Command");
			tool->WriteCache(%data);
		}

		for each(ModuleEditorInfo^ tool in editors)
		{
			data.Add("Editor");
			tool->WriteCache(%data);
		}

		for each(ModuleFilerInfo^ tool in filers)
		{
			data.Add("Filer");
			tool->WriteCache(%data);
		}

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
