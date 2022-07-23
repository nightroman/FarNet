
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Options for <see cref="IFar.ShowHelp"/>.
/// </summary>
[Flags]
public enum HelpOptions
{
	/// <summary>
	/// Show the topic from the help file of a DLL, the path is the DLL path.
	/// If a topic begins with a colon then the main Far help file is used and the path is ignored.
	/// </summary>
	None = 0x0,

	/// <summary>
	/// Path is ignored and the topic from the main Far help file is shown.
	/// In this case you do not need to start the topic with a colon ':'.
	/// </summary>
	Far = 1 << 0,

	/// <summary>
	/// Assume path specifies full path to a HLF file (c:\path\filename).
	/// </summary>
	File = 1 << 1,

	/// <summary>
	/// Assume path specifies full path to a folder (c:\path) from which
	/// a help file is selected according to current language settings.
	/// </summary>
	Path = 1 << 2,

	/// <summary>
	/// If the topic is not found, try to show the "Contents" topic.
	/// This flag can be combined with other flags.
	/// </summary>
	UseContents = 0x40000000,

	/// <summary>
	/// Disable file or topic not found error messages for this function call.
	/// This flag can be combined with other flags.
	/// </summary>
	NoError = unchecked((int)0x80000000),
}
