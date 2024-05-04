using FarNet;
using System.Xml.Serialization;

namespace RedisKit;

public class Settings : ModuleSettings<Settings.Data>
{
	public static Settings Default { get; } = new Settings();

	public class Data
	{
		public Configuration[] Configurations { get; set; } = [new()];
	}

	public class Configuration
	{
		[XmlAttribute]
		public string Name { get; set; } = "Main";

		[XmlText]
		public string Text { get; set; } = "%FARNET_REDIS_CONFIGURATION%";
	}
}

public class Workings : ModuleSettings<Workings.Data>
{
	public static Workings Default { get; } = new Workings();

	public Workings() : base(new ModuleSettingsArgs { IsLocal = true })
	{
	}

	public class Data
	{
		public string Configuration { get; set; } = "Main";
	}
}
