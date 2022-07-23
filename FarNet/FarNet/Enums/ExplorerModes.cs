
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Explorer call mode flags.
/// </summary>
[Flags]
public enum ExplorerModes
{
	///
	None = 0,

	/// <summary>
	/// A job should not interact with a user because it is a part of another job that does not want this.
	/// </summary>
	Silent = 0x0001,

	/// <summary>
	/// A job is called from a search or scan operation; screen output and user interaction should be avoided.
	/// </summary>
	Find = 0x0002,

	/// <summary>
	/// A job is a part of the file view operation.
	/// If a file is opened in the quick view panel, than the <c>View</c> and <c>QuickView</c> flags are both set.
	/// </summary>
	View = 0x0004,

	/// <summary>
	/// A job is a part of a file edit operation.
	/// </summary>
	Edit = 0x0008,

	/// <summary>
	/// All files in a host file of file based panel should be processed.
	/// This flag is set on [ShiftF2], [ShiftF3] commands outside of a host file.
	/// Passed in an operation file list also contains all necessary information,
	/// so that this flag can be either ignored or used to speed up processing.
	/// </summary>
	TopLevel = 0x0010,

	/// <summary>
	/// A job is called for files with file descriptions.
	/// </summary>
	Description = 0x0020,

	/// <summary>
	/// A job is a part of a file view operation in the quick view panel ([CtrlQ]).
	/// </summary>
	QuickView = 0x0040,
}
