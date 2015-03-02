
using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyTitle("Windows PowerShell host for FarNet")]
[assembly: AssemblyDescription("Implements PowerShell host and UI tools for FarNet")]
[assembly: AssemblyCopyright("Copyright (c) 2006-2015 Roman Kuzmin")]

[assembly: ComVisible(false)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, ViewAndModify = "HKEY_CURRENT_USER")]
[assembly: NeutralResourcesLanguage("en")]

[assembly: CLSCompliant(false)]
