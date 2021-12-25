
using System;
using System.ComponentModel;

namespace FarNet.Demo
{
	/// <summary>
	/// This class implements non-browsable local settings.
	/// </summary>
	/// <remarks>
	/// <para>
	/// To make settings non-browsable, use <c>[Browsable(false)]</c>.
	/// </para>
	/// <para>
	/// To make settings local, use the constructor arguments with <c>IsLocal</c> true.
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

		public class Data
		{
			/// <summary>
			/// Updated by the module host on loading.
			/// </summary>
			public DateTime LastLoadTime { get; set; }

			/// <summary>
			/// Updated by the module host on loading.
			/// </summary>
			public int LoadCount { get; set; }

			/// <summary>
			/// Updated by Scripts/Workings.far.ps1.
			/// </summary>
			public int MoreCount { get; set; }
		}
	}
}
