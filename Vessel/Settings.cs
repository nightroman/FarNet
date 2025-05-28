
using FarNet;

namespace Vessel;

public sealed class Settings : ModuleSettings<Settings.Data>
{
	public static Settings Default { get; } = new Settings();

	public class Data
	{
		public int MaximumDayCount { get; set; } = 42;

		public int MaximumFileAge { get; set; } = 365;

		public int MaximumFileCount { get; set; } = 1000;

		public int MaximumFileCountFromFar { get; set; } = 1000;

		public int MinimumRecentFileCount { get; set; } = 10;

		public int Limit0 { get; set; } = 2;

		public string ChoiceLog { get; set; } = string.Empty;
	}
}
