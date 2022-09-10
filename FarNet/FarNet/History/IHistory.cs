
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// History methods.
/// </summary>
public abstract class IHistory
{
	/// <summary>
	/// Gets the specified history.
	/// </summary>
	/// <param name="args">The arguments.</param>
	public abstract HistoryInfo[] GetHistory(GetHistoryArgs args);

	/// <summary>
	/// Returns command history.
	/// </summary>
	public HistoryInfo[] Command() => GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Command });

	/// <summary>
	/// Returns editor history.
	/// </summary>
	public HistoryInfo[] Editor() => GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Editor });

	/// <summary>
	/// Returns folder history.
	/// </summary>
	public HistoryInfo[] Folder() => GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Folder });

	/// <summary>
	/// Returns viewer history.
	/// </summary>
	public HistoryInfo[] Viewer() => GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Viewer });

	/// <summary>
	/// Returns dialog history with the specified name.
	/// </summary>
	/// <param name="name">The dialog history name.</param>
	public HistoryInfo[] Dialog(string name) => GetHistory(new GetHistoryArgs() { Name = name });
}
