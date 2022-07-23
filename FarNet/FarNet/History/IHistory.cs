// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// History operator.
/// </summary>
public abstract class IHistory
{
	/// <summary>
	/// Returns the specified history.
	/// </summary>
	/// <param name="args">The arguments.</param>
	public abstract HistoryInfo[] GetHistory(GetHistoryArgs args);

	/// <summary>
	/// Returns command history.
	/// </summary>
	public HistoryInfo[] Command()
	{
		return GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Command });
	}

	/// <summary>
	/// Returns editor history.
	/// </summary>
	public HistoryInfo[] Editor()
	{
		return GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Editor });
	}

	/// <summary>
	/// Returns folder history.
	/// </summary>
	public HistoryInfo[] Folder()
	{
		return GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Folder });
	}

	/// <summary>
	/// Returns viewer history.
	/// </summary>
	public HistoryInfo[] Viewer()
	{
		return GetHistory(new GetHistoryArgs() { Kind = HistoryKind.Viewer });
	}

	/// <summary>
	/// Returns dialog history with the specified name.
	/// </summary>
	/// <param name="name">The dialog history name.</param>
	public HistoryInfo[] Dialog(string name)
	{
		return GetHistory(new GetHistoryArgs() { Name = name });
	}
}
