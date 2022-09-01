
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Module drawer runtime representation.
/// </summary>
/// <remarks>
/// It represents an auto registered <see cref="ModuleDrawer"/> or a drawer registered by <see cref="IModuleManager.RegisterDrawer"/>.
/// </remarks>
public interface IModuleDrawer : IModuleAction
{
	/// <summary>
	/// Returns the drawer handler.
	/// </summary>
	Action<IEditor, ModuleDrawerEventArgs> CreateHandler();

	/// <summary>
	/// Gets the file mask. Setting is for internal use.
	/// </summary>
	string Mask { get; set; }

	/// <summary>
	/// Gets the priority. Setting is for internal use.
	/// </summary>
	int Priority { get; set; }
}
