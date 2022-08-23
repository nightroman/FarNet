
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

namespace FarNet
{
public ref class AssemblyResolver
{
public:
	static Assembly^ AssemblyResolve(Object^ sender, ResolveEventArgs^ args);
};
}
