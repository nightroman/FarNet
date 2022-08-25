
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

	// load FarNet assemblies explicitly

	if (name == "FarNet")
		return _FarNet;

	if (name == "FarNet.Works.Config")
	{
		Trace::WriteLine("farnet FarNet.Works.Config");
		if (!_FarNetConfig)
			_FarNetConfig = Assembly::LoadFrom(Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\FarNet.Works.Config.dll"));
		return _FarNetConfig;
	}

	if (name == "FarNet.Works.Dialog")
	{
		Trace::WriteLine("farnet FarNet.Works.Dialog");
		if (!_FarNetDialog)
			_FarNetDialog = Assembly::LoadFrom(Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\FarNet.Works.Dialog.dll"));
		return _FarNetDialog;
	}

	if (name == "FarNet.Works.Editor")
	{
		Trace::WriteLine("farnet FarNet.Works.Editor");
		if (!_FarNetEditor)
			_FarNetEditor = Assembly::LoadFrom(Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\FarNet.Works.Editor.dll"));
		return _FarNetEditor;
	}

	if (name == "FarNet.Works.Panels")
	{
		Trace::WriteLine("farnet FarNet.Works.Panels");
		if (!_FarNetPanels)
			_FarNetPanels = Assembly::LoadFrom(Environment::ExpandEnvironmentVariables("%FARHOME%\\FarNet\\FarNet.Works.Panels.dll"));
		return _FarNetPanels;
	}

	// resolve other assemblies
	return Works::AssemblyResolver::ResolveAssembly(name, args);
}
}
