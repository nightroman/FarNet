using System;
using System.ComponentModel;

namespace FarNet.Demo
{
	/// <summary>
	/// This class implements non-browsable local settings.
	/// </summary>
	/// <remarks>
	/// <para>
	/// To make settings non-browsable, use the attribute <see cref="BrowsableAttribute"/> with false.
	/// </para>
	/// <para>
	/// To make settings local, use the constructor arguments with <see cref="ModuleSettingsArgs.IsLocal"/> set to true.
	/// </para>
	/// <para>
	/// Unlike <see cref="Settings"/>, this class does not use the static instance with cached data.
	/// These settings are read from the file on <see cref="ModuleSettings{T}.GetData"/>, then updated and saved.
	/// </para>
	/// </remarks>
	[Browsable(false)]
	public class Workings : ModuleSettings<Workings.Data>
	{
		public Workings() : base(new ModuleSettingsArgs { IsLocal = true })
		{ }

		[Serializable]
		public class Data
		{
			public DateTime LastLoadTime { get; set; }

			public int LoadCount { get; set; }
		}
	}
}
