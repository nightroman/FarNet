
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
#include "AssemblyResolver.h"

namespace FarNet
{
void AssemblyResolver::Init()
{
	_FarNet = Assembly::LoadFrom(Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\FarNet.dll"));
	AppDomain::CurrentDomain->AssemblyResolve += gcnew ResolveEventHandler(AssemblyResolve);
}

Assembly^ AssemblyResolver::AssemblyResolve(Object^ /*sender*/, ResolveEventArgs^ args)
{
	// skip no caller, e.g. XmlSerializers
	if (!args->RequestingAssembly)
		return nullptr;

	// skip .resources for now
	auto name = args->Name->Substring(0, args->Name->IndexOf(','));
	if (name->EndsWith(".resources"))
		return nullptr;

	// load FarNet explicitly
	if (name == "FarNet")
		return _FarNet;

	// resolve other assemblies
	return Works::AssemblyResolver::ResolveAssembly(name, args);
}
}
