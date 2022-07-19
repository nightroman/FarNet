
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

namespace FarNet
{
public ref class AssemblyResolver
{
public:
	static void Init();
	static Assembly^ AssemblyResolve(Object^ sender, ResolveEventArgs^ args);
	static Assembly^ ResolvePowerShellFar(String^ root, ResolveEventArgs^ args);

private:
	static String^ AssemblyNameToDllName(String^ name);
	static void AddRoot(String^ name);
	static bool IsRoot(String^ name);

	static Dictionary<String^, Object^>^ _cache;
	static LinkedList<String^> _roots;
};
}
