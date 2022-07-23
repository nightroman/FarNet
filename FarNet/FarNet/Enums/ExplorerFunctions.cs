
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Defines the functions that are either implemented or supported by the explorer.
/// </summary>
[Flags]
public enum ExplorerFunctions
{
	///
	None,

	/// <summary>
	/// It implements <see cref="Explorer.ExploreLocation"/>, works with pure paths, i.e. files without <see cref="FarFile.Data"/>.
	/// </summary>
	ExploreLocation = 1 << 0,

	/// <summary>
	/// It implements <see cref="Explorer.AcceptFiles"/>.
	/// </summary>
	AcceptFiles = 1 << 1,

	/// <summary>
	/// It implements <see cref="Explorer.ImportFiles"/>.
	/// </summary>
	ImportFiles = 1 << 2,

	/// <summary>
	/// It implements <see cref="Explorer.ExportFiles"/>.
	/// </summary>
	ExportFiles = 1 << 3,

	/// <summary>
	/// It implements <see cref="Explorer.DeleteFiles"/>.
	/// </summary>
	DeleteFiles = 1 << 4,

	/// <summary>
	/// It implements <see cref="Explorer.CreateFile"/>.
	/// </summary>
	CreateFile = 1 << 5,

	/// <summary>
	/// It implements <see cref="Explorer.GetContent"/>.
	/// </summary>
	GetContent = 1 << 6,

	/// <summary>
	/// It implements <see cref="Explorer.SetFile"/>.
	/// </summary>
	SetFile = 1 << 7,

	/// <summary>
	/// It implements <see cref="Explorer.SetText"/>.
	/// </summary>
	SetText = 1 << 8,

	/// <summary>
	/// It implements <see cref="Explorer.OpenFile"/>.
	/// </summary>
	OpenFile = 1 << 9,

	/// <summary>
	/// It implements <see cref="Explorer.CloneFile"/>.
	/// </summary>
	CloneFile = 1 << 10,

	/// <summary>
	/// It implements <see cref="Explorer.RenameFile"/>.
	/// </summary>
	RenameFile = 1 << 11,
}
