using System;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]

namespace PowerShellFar;

/// <summary>
/// GUID strings
/// </summary>
public static class Guids
{
	/// <summary>
	/// GUID
	/// </summary>
	public const string
		AssertDialog = "0dc6e477-8aed-46dd-86e3-0331f2a1b4eb",
		InputBoxEx = "20c78c35-c91c-4153-a139-86012611c924",
		InputDialog = "416ff960-9b6b-4f3f-8bda-0c9274c75e53",
		ObjectExplorer = "07e4dde7-e113-4622-b2e9-81cf3cda927a",
		PSPromptDialog = "d9a8d41b-053e-4ec0-a03a-7b8eb28ef973",
		ReadCommandDialog = "25b66eb8-14de-4894-94e4-02a6da03f75e",
		ReadLineDialog = "ce59fc98-546d-43d8-98f3-1e1b122cf9a5";
}

/// <summary>
/// Parameter names.
/// </summary>
static class Prm
{
	public const string
		Confirm = "Confirm",
		ErrorAction = "ErrorAction",
		Force = "Force",
		Recurse = "Recurse";
}
