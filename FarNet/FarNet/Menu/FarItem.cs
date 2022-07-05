﻿
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet
{
	/// <summary>
	/// Item of a menu, a list menu or one of list dialog controls.
	/// </summary>
	/// <seealso cref="IMenu"/>
	/// <seealso cref="IListMenu"/>
	/// <seealso cref="Forms.IListBox"/>
	/// <seealso cref="Forms.IComboBox"/>
	public abstract class FarItem
	{
		/// <summary>
		/// Item text.
		/// </summary>
		public abstract string Text { get; set; }

		/// <summary>
		/// Item is checked.
		/// </summary>
		public virtual bool Checked { get { return false; } set { throw new NotImplementedException(); } }

		/// <summary>
		/// Item is disabled. It cannot be selected.
		/// </summary>
		public virtual bool Disabled { get { return false; } set { throw new NotImplementedException(); } }

		/// <summary>
		/// Item is shown, but cannot be selected.
		/// </summary>
		public virtual bool Grayed { get { return false; } set { throw new NotImplementedException(); } }

		/// <summary>
		/// Item is hidden. It cannot be selected.
		/// </summary>
		public virtual bool Hidden { get { return false; } set { throw new NotImplementedException(); } }

		/// <summary>
		/// Item is a separator. <see cref="Text"/>, if any, is shown center aligned.
		/// </summary>
		public virtual bool IsSeparator { get { return false; } set { throw new NotImplementedException(); } }

		/// <summary>
		/// Any user data attached to the item.
		/// </summary>
		public virtual object Data { get { return null; } set { throw new NotImplementedException(); } }

		/// <summary>
		/// Called when a menu item is selected.
		/// </summary>
		public virtual EventHandler<MenuEventArgs> Click { get { return null; } set { throw new NotImplementedException(); } }
	}
}
