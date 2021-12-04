
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet
{
	/// <summary>
	/// Settings arguments.
	/// </summary>
	public class ModuleSettingsArgs
	{
		/// <summary>
		/// Tells to use the local settings file.
		/// </summary>
		public bool IsLocal { get; set; }

		/// <summary>
		/// Tells to use the spefified file path.
		/// </summary>
		public string FileName { get; set; }
	}
}
