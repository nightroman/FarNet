
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Panel.ViewChanged"/> event. [FE_CHANGEVIEWMODE], [Column types].
/// </summary>
/// <param name="columns">See <see cref="Columns"/></param>
public sealed class ViewChangedEventArgs(string columns) : PanelEventArgs
{
	/// <summary>
	/// Gets column kinds, e.g. N,S,D,T.
	/// </summary>
	public string Columns { get; } = columns;
}
