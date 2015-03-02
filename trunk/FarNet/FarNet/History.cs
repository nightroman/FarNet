
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet
{
	/// <summary>
	/// History operator.
	/// </summary>
	public abstract class IHistory
	{
		/// <summary>
		/// Returns command history.
		/// </summary>
		public abstract HistoryInfo[] Command();
		/// <summary>
		/// Returns dialog history (edit box history).
		/// </summary>
		/// <param name="name">The dialog history name.</param>
		public abstract HistoryInfo[] Dialog(string name);
		/// <summary>
		/// Returns editor history.
		/// </summary>
		public abstract HistoryInfo[] Editor();
		/// <summary>
		/// Returns folder history.
		/// </summary>
		public abstract HistoryInfo[] Folder();
		/// <summary>
		/// Returns viewer history.
		/// </summary>
		public abstract HistoryInfo[] Viewer();
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
