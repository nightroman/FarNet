
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Arguments of <see cref="IHistory.GetHistory"/>.
/// </summary>
public class GetHistoryArgs
{
	/// <summary>
	/// Specifies command, folder, editor, viewer.
	/// </summary>
	public HistoryKind Kind { get; set; }

	/// <summary>
	/// Specifies the dialog history name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Tells to take the specified number of last items.
	/// </summary>
	public int Last { get; set; }
}
