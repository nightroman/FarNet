using FarNet;
using System.Xml.Serialization;

namespace EditorKit;

public class Settings : ModuleSettings<Settings.Data>
{
	public static Settings Default { get; } = new Settings();

	public class Data
	{
		public ColorerType[] ColorerTypes { get; set; } = [new() { Type = "json", Mask = "*.canvas" }];
	}

	public class ColorerType
	{
		[XmlAttribute]
		public required string Type { get; set; }

		[XmlAttribute]
		public required string Mask { get; set; }

		[XmlAttribute]
		public bool Full { get; set; }
	}
}
