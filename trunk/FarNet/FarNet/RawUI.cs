/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

namespace FarNet
{
	/// <summary>
	/// "Low level" UI.
	/// </summary>
	public abstract class IRawUI
	{
		/// <summary>
		/// Gets the Far window place in the buffer.
		/// </summary>
		public abstract Place WindowPlace { get; }
		/// <summary>
		/// Gets or sets the cursor position in the Far window.
		/// </summary>
		public abstract Point WindowCursor { get; set; }
	}
}
