using FarNet;
using System.ComponentModel;

namespace Vessel;

[Browsable(false)]
public sealed class Workings : ModuleSettings<Workings.Data>
{
	public Workings() : base(new ModuleSettingsArgs { IsLocal = true })
	{
	}

	public class Data
	{
		/// <summary>
		/// History log last update time.
		/// </summary>
		public DateTime LastUpdateTime1 { get; set; }

		/// <summary>
		/// Folders log last update time.
		/// </summary>
		public DateTime LastUpdateTime2 { get; set; }

		/// <summary>
		/// Commands log last update time.
		/// </summary>
		public DateTime LastUpdateTime3 { get; set; }
	}
}
