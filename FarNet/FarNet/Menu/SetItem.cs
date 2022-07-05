
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
	public sealed class SetItem : FarItem
	{
		/// <summary>
		/// Item text.
		/// </summary>
		public override string Text { get; set; }

		/// <summary>
		/// Item is checked.
		/// </summary>
		public override bool Checked { get; set; }

		/// <summary>
		/// Item is disabled. It cannot be selected.
		/// </summary>
		public override bool Disabled { get; set; }

		/// <summary>
		/// Item is shown, but cannot be selected.
		/// </summary>
		public override bool Grayed { get; set; }

		/// <summary>
		/// Item is hidden. It cannot be selected.
		/// </summary>
		public override bool Hidden { get; set; }

		/// <summary>
		/// Item is a separator. <see cref="Text"/>, if any, is shown center aligned.
		/// </summary>
		public override bool IsSeparator { get; set; }

		/// <summary>
		/// Any user data attached to the item.
		/// </summary>
		public override object Data { get; set; }

		/// <summary>
		/// Called when a menu item is clicked.
		/// </summary>
		public override EventHandler<MenuEventArgs> Click { get; set; }
	}
}
