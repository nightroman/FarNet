
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet
{
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

	/// <summary>
	/// Supported history kinds.
	/// </summary>
	public enum HistoryKind
	{
		///
		Command = 1,
		///
		Folder = 2,
		///
		Viewer = 3,
		///
		Editor = 4,
	}

	/// <summary>
	/// History information.
	/// </summary>
	public sealed class HistoryInfo
	{
		/// <param name="name">See <see cref="Name"/></param>
		/// <param name="time">See <see cref="Time"/></param>
		/// <param name="isLocked">See <see cref="IsLocked"/></param>
		public HistoryInfo(string name, DateTime time, bool isLocked)
		{
			Name = name;
			Time = time;
			IsLocked = isLocked;
		}
		/// <summary>
		/// History information, text.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Last time.
		/// </summary>
		public DateTime Time { get; private set; }
		/// <summary>
		/// Locked state.
		/// </summary>
		public bool IsLocked { get; private set; }
	}
}
