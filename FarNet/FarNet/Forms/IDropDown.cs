
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms
{
	/// <summary>
	/// Common members of controls with a drop down box, i.e. edit boxes and combo boxes.
	/// </summary>
	public interface IDropDown
	{
		/// <summary>
		/// Called when a drop down list is opening.
		/// </summary>
		event EventHandler<DropDownOpeningEventArgs> DropDownOpening;

		/// <summary>
		/// Called when a drop down list is closed.
		/// </summary>
		event EventHandler<DropDownClosedEventArgs> DropDownClosed;
	}
}
