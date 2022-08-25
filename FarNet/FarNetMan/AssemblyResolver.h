
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

namespace FarNet
{
public ref class AssemblyResolver
{
public:
	static void Init();
private:
	static Assembly^ AssemblyResolve(Object^ sender, ResolveEventArgs^ args);
private:
	static Assembly^ _FarNet;
	static Assembly^ _FarNetConfig;
	static Assembly^ _FarNetDialog;
	static Assembly^ _FarNetEditor;
	static Assembly^ _FarNetPanels;
};
}
