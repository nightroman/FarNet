
#include "stdafx.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

[assembly: AssemblyVersion("5.0.33.0")];
[assembly: AssemblyTitle("FarNet plugin manager")];
[assembly: AssemblyDescription("FarNet plugin manager")];
[assembly: AssemblyCompany("http://code.google.com/p/farnet/")];
[assembly: AssemblyProduct("FarNet")];
[assembly: AssemblyCopyright("Copyright (c) 2006-2013 Roman Kuzmin")];

[assembly: ComVisible(false)];
[assembly: CLSCompliant(true)];
[assembly: SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)];
