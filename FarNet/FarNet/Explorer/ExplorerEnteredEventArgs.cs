
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of the <see cref="Panel.UIExplorerEntered"/> method and the <see cref="Panel.ExplorerEntered"/> event.
/// </summary>
/// <param name="explorer">See <see cref="Explorer"/></param>
public sealed class ExplorerEnteredEventArgs(Explorer explorer) : EventArgs
{
	/// <summary>
	/// The old explorer replaced by the new just entered <see cref="Panel.Explorer"/>.
	/// </summary>
	public Explorer Explorer { get; } = explorer;
}
