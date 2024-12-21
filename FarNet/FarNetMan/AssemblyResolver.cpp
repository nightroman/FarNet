
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
#include "AssemblyResolver.h"

namespace FarNet
{
void AssemblyResolver::Init()
{
	// load FarNet explicitly
	Assembly::LoadFrom(Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\FarNet.dll"));

	//! add C++/CLI handler, adding C# handler will crash
	AppDomain::CurrentDomain->AssemblyResolve += gcnew ResolveEventHandler(AssemblyResolve);
}

Assembly^ AssemblyResolver::AssemblyResolve(Object^ /*sender*/, ResolveEventArgs^ args)
{
	return Works::AssemblyResolver::ResolveAssembly(args);
}
}
