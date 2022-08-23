
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
#include "AssemblyResolver.h"
#include "Far0.h"

namespace FarNet
{
Assembly^ AssemblyResolver::AssemblyResolve(Object^ /*sender*/, ResolveEventArgs^ args)
{
	// skip no caller, e.g. XmlSerializers
	if (!args->RequestingAssembly)
		return nullptr;

	// skip .resources for now
	auto name = args->Name->Substring(0, args->Name->IndexOf(','));
	if (name->EndsWith(".resources"))
		return nullptr;

	// load known FarNet explicitly
	if (name == "FarNet" ||
		name == "FarNet.Works.Config" ||
		name == "FarNet.Works.Dialog" ||
		name == "FarNet.Works.Editor" ||
		name == "FarNet.Works.Panels")
	{
		Trace::WriteLine("farnet " + name);
		return Assembly::LoadFrom(Environment::GetEnvironmentVariable("FARHOME") + "\\FarNet\\" + name + ".dll");
	}

	//! cannot reference anything from FarNet directly, use this "delayed" method
	return Far0::ResolveAssembly(name, args);
}
}
