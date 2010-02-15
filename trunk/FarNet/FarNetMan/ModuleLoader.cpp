/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "ModuleLoader.h"
#include "Far0.h"
#include "ModuleManager.h"

namespace FarNet
{;
// #1 Load all
void ModuleLoader::LoadModules()
{
	ReadCache();

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

// #2 Load cache
void ModuleLoader::ReadCache()
{
	RegistryKey^ keyCache;
	try
	{
		keyCache = Registry::CurrentUser->CreateSubKey(Far::Net->RegistryPluginsPath + "\\FarNet\\<cache>");
		for each (String^ dllName in keyCache->GetSubKeyNames())
		{
			bool ok = true;
			RegistryKey^ keyDll;
			List<ModuleCommandInfo^> commands;
			List<ModuleEditorInfo^> editors;
			List<ModuleFilerInfo^> filers;
			List<ModuleToolInfo^> tools;
			try
			{
				keyDll = keyCache->OpenSubKey(dllName);

				String^ assemblyPath = keyDll->GetValue("Path", String::Empty)->ToString();
				if (!assemblyPath->Length || !File::Exists(assemblyPath))
					throw gcnew OperationCanceledException;

				String^ assemblyStamp = keyDll->GetValue("Stamp", String::Empty)->ToString();
				FileInfo fi(assemblyPath);
				if (assemblyStamp != fi.LastWriteTime.Ticks.ToString(CultureInfo::InvariantCulture))
				{
					// do not throw, it's PITA in debugging
					ok = false;
				}
				else
				{
					ModuleManager^ manager = gcnew ModuleManager(assemblyPath);
					for each (String^ className in keyDll->GetSubKeyNames())
					{
						RegistryKey^ keyEntry = keyDll->OpenSubKey(className);
						try
						{
							// get entry type
							String^ entryType = keyEntry->GetValue("Type", String::Empty)->ToString();
							if (!entryType->Length)
								throw gcnew OperationCanceledException;

							// host:
							if (entryType == "Host")
							{
								manager->SetModuleHost(className);
								continue;
							}

							// get entry name
							String^ entryName = keyEntry->GetValue("Name", String::Empty)->ToString();
							if (!entryName->Length)
								throw gcnew OperationCanceledException;

							// types:
							if (entryType == "Tool")
							{
								int options = (int)keyEntry->GetValue("Options");
								if (!options)
									throw gcnew OperationCanceledException;

								ModuleToolInfo^ tool = gcnew ModuleToolInfo(manager, className, entryName, (ModuleToolOptions)options);
								tools.Add(tool);
							}
							else if (entryType == "Command")
							{
								String^ prefix = keyEntry->GetValue("Prefix", String::Empty)->ToString();
								if (!prefix->Length)
									throw gcnew OperationCanceledException;

								ModuleCommandInfo^ tool = gcnew ModuleCommandInfo(manager, className, entryName, prefix);
								commands.Add(tool);
							}
							else if (entryType == "Editor")
							{
								String^ mask = keyEntry->GetValue("Mask", String::Empty)->ToString();

								ModuleEditorInfo^ tool = gcnew ModuleEditorInfo(manager, className, entryName, mask);
								editors.Add(tool);
							}
							else if (entryType == "Filer")
							{
								String^ mask = keyEntry->GetValue("Mask", String::Empty)->ToString();
								int creates = (int)keyEntry->GetValue("Creates", (Object^)-1);

								ModuleFilerInfo^ tool = gcnew ModuleFilerInfo(manager, className, entryName, mask, creates != 0);
								filers.Add(tool);
							}
							else
							{
								throw gcnew OperationCanceledException;
							}
						}
						finally
						{
							keyEntry->Close();
						}
					}

					keyDll->Close();

					// add dllName to dictionary and add plugins
					_ModuleManagers->Add(dllName, manager);
					if (commands.Count)
						Far0::RegisterCommands(%commands);
					if (editors.Count)
						Far0::RegisterEditors(%editors);
					if (filers.Count)
						Far0::RegisterFilers(%filers);
					if (tools.Count)
						Far0::RegisterTools(%tools);
				}
			}
			catch(OperationCanceledException^)
			{
				ok = false;
			}
			catch(Exception^ ex)
			{
				throw gcnew OperationCanceledException(
					"Error on reading the cache. Remove registry FarNet\\<cache> manually and restart Far.", ex);
			}

			// error or outdated info
			if (!ok)
			{
				if (keyDll)
					keyDll->Close();
				keyCache->DeleteSubKeyTree(dllName);
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
			throw gcnew OperationCanceledException("More than one .cfg files found.");

		// the assembly
		array<String^>^ assemblies = Directory::GetFiles(dir, "*.dll");
		if (assemblies->Length > 1)
			throw gcnew OperationCanceledException("More than one .dll files found. Expected exactly one .dll file or exactly one .cfg file telling the .dll name.");
		if (assemblies->Length < 1)
			throw gcnew OperationCanceledException("The module folder has no .dll or .cfg files.");
		LoadFromAssembly(assemblies[0], nullptr);
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
		throw gcnew OperationCanceledException("The manifest file is empty.");
	
	// assembly
	String^ path = lines[0]->TrimEnd();
	if (path->Length == 0)
		throw gcnew OperationCanceledException("Expected the module assembly name as the first line of the manifest file.");
	path = Path::Combine(dir, path);
	
	// classes
	List<String^> classes(lines->Length - 1);
	for(int i = 1; i < lines->Length; ++i)
	{
		String^ name = lines[i]->Trim();
		if (name->Length)
			classes.Add(name);
	}
	
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
		int nLoaded = 0;
		for each(Type^ type in assembly->GetExportedTypes())
		{
			if (type->IsAbstract)
				continue;
			
			if (BaseModuleEntry::typeid->IsAssignableFrom(type))
			{
				++nLoaded;
				AddModuleEntry(manager, type, %commands, %editors, %filers, %tools);
			}
		}

		if (nLoaded == 0)
			throw gcnew InvalidOperationException("Module '" + assemblyPath + "' has no suitable entry classes.");
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

	// write cache
	if (!manager->GetLoadedModuleHost())
		WriteCache(manager, %commands, %editors, %filers, %tools);
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
		throw gcnew InvalidOperationException();
	}
}

//! Don't use Far UI
//! Don't use Far UI
void ModuleLoader::UnloadEntry(BaseModuleEntry^ entry)
{
	LOG_AUTO(3, "Unload module entry " + entry);

	// tool:
	BaseModuleTool^ tool = dynamic_cast<BaseModuleTool^>(entry);
	if (tool)
		return;

	ModuleHost^ host = (ModuleHost^)entry;
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

void ModuleLoader::WriteCache(ModuleManager^ manager, List<ModuleCommandInfo^>^ commands, List<ModuleEditorInfo^>^ editors, List<ModuleFilerInfo^>^ filers, List<ModuleToolInfo^>^ tools)
{
	FileInfo fi(manager->AssemblyPath);
	RegistryKey^ keyDll;
	try
	{
		keyDll = Registry::CurrentUser->CreateSubKey(Far::Net->RegistryPluginsPath + "\\FarNet\\<cache>\\" + fi.Name);
		keyDll->SetValue("Path", manager->AssemblyPath);
		keyDll->SetValue("Stamp", fi.LastWriteTime.Ticks.ToString(CultureInfo::InvariantCulture));

		// late host
		String^ hostClassName = manager->GetModuleHostClassName();
		if (hostClassName)
		{
			RegistryKey^ key = keyDll->CreateSubKey(hostClassName);
			key->SetValue("Type", "Host");
			key->Close();
		}

		for each(ModuleToolInfo^ tool in tools)
		{
			RegistryKey^ key = keyDll->CreateSubKey(tool->ClassName);
			key->SetValue("Type", "Tool");
			key->SetValue("Name", tool->Name);
			key->SetValue("Options", (int)tool->Options);
			key->Close();
		}

		for each(ModuleCommandInfo^ tool in commands)
		{
			RegistryKey^ key = keyDll->CreateSubKey(tool->ClassName);
			key->SetValue("Type", "Command");
			key->SetValue("Name", tool->Name);
			key->SetValue("Prefix", tool->DefaultPrefix);
			key->Close();
		}

		for each(ModuleEditorInfo^ tool in editors)
		{
			RegistryKey^ key = keyDll->CreateSubKey(tool->ClassName);
			key->SetValue("Type", "Editor");
			key->SetValue("Name", tool->Name);
			key->SetValue("Mask", tool->DefaultMask);
			key->Close();
		}

		for each(ModuleFilerInfo^ tool in filers)
		{
			RegistryKey^ key = keyDll->CreateSubKey(tool->ClassName);
			key->SetValue("Type", "Filer");
			key->SetValue("Name", tool->Name);
			key->SetValue("Mask", tool->DefaultMask);
			key->SetValue("Creates", (int)tool->Creates);
			key->Close();
		}
	}
	finally
	{
		if (keyDll)
			keyDll->Close();
	}
}

}
