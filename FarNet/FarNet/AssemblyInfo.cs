using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: InternalsVisibleTo("FarNetTest")]

#if DEBUG
[assembly: AssemblyDescription("FarNet API (DEBUG)")]
#else
[assembly: AssemblyDescription("FarNet API")]
#endif
