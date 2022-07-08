
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
#include "AssemblyResolver.h"

namespace FarNet
{
// examples used to have issues:
// 1) Microsoft.Bcl.AsyncInterfaces in Lib\FarNet.CsvHelper, Modules\PowerShellFar
// 2) System.Data.OleDb.dll in Lib\FarNet.FSharp.Charting (2) Modules\FolderChart (2) Modules\PowerShellFar (2)
void AssemblyResolver::Init()
{
	_cache = gcnew Dictionary<String^, Object^>(StringComparer::OrdinalIgnoreCase);

	auto root = Environment::GetEnvironmentVariable("FARHOME") + "\\FarNet";
	for each (auto path in Directory::EnumerateFiles(root, "*.dll", SearchOption::AllDirectories))
	{
		auto key = Path::GetFileNameWithoutExtension(path);

		// add, if 2+ null it
		if (!_cache->TryAdd(key, path))
			_cache[key] = nullptr;
	}
}

String^ AssemblyResolver::AssemblyNameToDllName(String^ name)
{
	return name->Substring(0, name->IndexOf(",")) + ".dll";
}

bool AssemblyResolver::IsRoot(String^ name)
{
	return (name->Contains("\\FarNet\\Modules") || name->Contains("\\FarNet\\Lib")) && !name->Contains("\\runtimes");
}

void AssemblyResolver::AddRoot(String^ name)
{
	if (_roots.Count > 0 && _roots.First->Value == name)
		return;

	_roots.Remove(name);
	_roots.AddFirst(name);
	while (_roots.Count > 4)
		_roots.RemoveLast();
}

Assembly^ AssemblyResolver::ResolvePowerShellFar(String^ root, ResolveEventArgs^ args)
{
	// frequently called and missing
	if (args->Name->StartsWith("System.Management.Automation.resources"))
		return nullptr;

	auto caller = args->RequestingAssembly->FullName;
	auto dllName = AssemblyNameToDllName(args->Name);

	if (caller->StartsWith("System.Management.Automation") || caller->StartsWith("PowerShellFar"))
	{
		// most frequent
		// PowerShellFar ->
		//   System.Management.Automation
		// System.Management.Automation ->
		//   Microsoft.PowerShell.ConsoleHost
		//   Microsoft.PowerShell.Commands.Utility
		//   Microsoft.PowerShell.Commands.Management
		//   Microsoft.PowerShell.Security
		auto path = root + "\\runtimes\\win\\lib\\net6.0\\" + dllName;
		if (File::Exists(path))
			return Assembly::LoadFrom(path);
	}

	if (caller->StartsWith("System.Management.Automation"))
	{
		// System.Management.Automation ->
		//   System.Management
		auto path = root + "\\" + dllName;
		if (File::Exists(path))
			return Assembly::LoadFrom(path);
	}

	if (caller->StartsWith("Microsoft.PowerShell.Commands.Management"))
	{
		// Microsoft.PowerShell.Commands.Management ->
		//   Microsoft.Management.Infrastructure
		auto win10_x64 = System::Runtime::InteropServices::RuntimeInformation::RuntimeIdentifier;
		auto path = root + "\\runtimes\\" + win10_x64 + "\\lib\\netstandard1.6\\" + dllName;
		if (File::Exists(path))
			return Assembly::LoadFrom(path);
	}

	return nullptr;
}

Assembly^ AssemblyResolver::AssemblyResolve(Object^ /*sender*/, ResolveEventArgs^ args)
{
	// e.g. FarNet.XmlSerializers
	if (!args->RequestingAssembly)
		return nullptr;

	auto name = args->Name->Substring(0, args->Name->IndexOf(","));

	// skip missing in FarNet
	Object^ value;
	if (!_cache->TryGetValue(name, value))
		return nullptr;

	// single in FarNet, load once
	if (value)
	{
		auto assembly = dynamic_cast<Assembly^>(value);
		if (!assembly)
		{
			assembly = Assembly::LoadFile(safe_cast<String^>(value));
			_cache[name] = assembly;
		}

		auto location = assembly->Location;
		if (IsRoot(location))
			AddRoot(Path::GetDirectoryName(location));

		return assembly;
	}

	String^ dllName = nullptr;
	if (!args->RequestingAssembly->IsDynamic)
	{
		auto callerFile = args->RequestingAssembly->Location;

		// case: PowerShellFar
		int index = callerFile->LastIndexOf("\\PowerShellFar\\");
		if (index > 0)
			return ResolvePowerShellFar(callerFile->Substring(0, index + 15), args);

		// case: same folder as the caller
		auto callerRoot = Path::GetDirectoryName(callerFile);
		dllName = AssemblyNameToDllName(args->Name);
		auto path = callerRoot + "\\" + dllName;
		if (File::Exists(path))
		{
			if (IsRoot(callerRoot))
				AddRoot(callerRoot);

			return Assembly::LoadFrom(path);
		}
	}

	// case: same folder as last roots
	dllName = dllName ? dllName : AssemblyNameToDllName(args->Name);
	for each (auto root in _roots)
	{
		auto path = root + "\\" + dllName;
		if (File::Exists(path))
		{
			AddRoot(root);
			return Assembly::LoadFrom(path);
		}
	}

	return nullptr;
}
}
