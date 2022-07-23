
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="Panel.ViewChanged"/> event. [FE_CHANGEVIEWMODE], [Column types].
/// </summary>
public sealed class ViewChangedEventArgs : PanelEventArgs
{
	/// <param name="columns">See <see cref="Columns"/></param>
	public ViewChangedEventArgs(string columns)
	{
		Columns = columns;
	}

	/// <summary>
	/// Gets column kinds, e.g. N,S,D,T.
	/// </summary>
	public string Columns { get; }
}
