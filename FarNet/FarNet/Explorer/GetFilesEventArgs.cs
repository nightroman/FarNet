
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// <see cref="Explorer.GetFiles"/> arguments.
/// </summary>
public class GetFilesEventArgs : ExplorerEventArgs
{
	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	public GetFilesEventArgs(ExplorerModes mode) : base(mode)
	{
	}

	/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
	/// <param name="offset">See <see cref="Offset"/></param>
	/// <param name="limit">See <see cref="Limit"/></param>
	/// <param name="newFiles">See <see cref="NewFiles"/></param>
	public GetFilesEventArgs(ExplorerModes mode, int offset, int limit, bool newFiles) : base(mode)
	{
		Limit = limit;
		Offset = offset;
		NewFiles = newFiles;
	}

	/// <summary>
	/// Gets the maximum number of files to get on paging.
	/// </summary>
	public int Limit { get; }

	/// <summary>
	/// Gets the number of files to skip on paging.
	/// </summary>
	public int Offset { get; }

	/// <summary>
	/// Tells to gets new (not cached) files, for example on paging.
	/// </summary>
	public bool NewFiles { get; }
}
