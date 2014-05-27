
#include "stdafx.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

#include "AssemblyMeta.h"

[assembly: ComVisible(false)];
[assembly: CLSCompliant(true)];
[assembly: SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)];
