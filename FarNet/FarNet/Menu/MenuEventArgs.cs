
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet
{
	/// <summary>
	/// Arguments of a menu key handler.
	/// By default the key closes the menu and it is stored in <see cref="IAnyMenu.Key"/>.
	/// Use <see cref="Ignore"/> or <see cref="Restart"/> to perform different actions.
	/// </summary>
	public class MenuEventArgs : EventArgs
	{
		/// <param name="item">Current item.</param>
		public MenuEventArgs(FarItem item)
		{
			_Item = item;
		}

		readonly FarItem _Item;

		/// <summary>
		/// Current item.
		/// </summary>
		public FarItem Item
		{
			get { return _Item; }
		}

		/// <summary>
		/// Tells to do nothing, a handler has processed everything.
		/// </summary>
		public bool Ignore { get; set; }

		/// <summary>
		/// Tells to restart the menu, normally when items or properties are changed.
		/// In some cases you may want to set proper <see cref="IAnyMenu.Selected"/> or -1
		/// (e.g. you recreated all items and want the first or the last to be current after that).
		/// </summary>
		public bool Restart { get; set; }
	}
}
