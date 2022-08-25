
using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace FarNet.Demo;

/// <summary>
/// This class implements browsable roaming settings.
/// </summary>
/// <remarks>
/// The type name is used as the settings file name and as the display name in the settings menu.
/// Specify the template parameter type <c>Settings.Data</c> which defines the stored settings.
/// </remarks>
/// <seealso cref="Workings"/>
public class Settings : ModuleSettings<Settings.Data>
{
	/// <summary>
	/// The current data version.
	/// </summary>
	const int CurrentVersion = 1;

	/// <summary>
	/// This cached static object is known to FarNet due to the conventional name <c>Default</c>.
	/// When you edit settings from the FarNet settings menu this object is updated automatically.
	/// </summary>
	public static Settings Default { get; } = new Settings();

	/// <summary>
	/// This class defines the settings data.
	/// </summary>
	/// <remarks>
	/// Optionally use <see cref="XmlRootAttribute"/> for the root name.
	/// Optionally implement <see cref="IValidate"/> for data validation.
	/// </remarks>
	[XmlRoot("Data")]
	public class Data : IValidate
	{
		internal int SavedVersion;

		/// <summary>
		/// This property is for serialization only.
		/// Use <see cref="SavedVersion"/> and <see cref="CurrentVersion"/> instead.
		/// </summary>
		public int Version
		{
			get => CurrentVersion;
			set => SavedVersion = value;
		}

		/// <summary>
		/// Some string.
		/// </summary>
		public string Name { get; set; } = "John Doe";

		/// <summary>
		/// Some number.
		/// </summary>
		public int Age { get; set; } = 42;

		/// <summary>
		/// CDATA with empty text.
		/// </summary>
		public XmlCData Memo { get; set; }

		/// <summary>
		/// CDATA with some text.
		/// </summary>
		public XmlCData Regex { get; set; } = "([<>&]+)";

		/// <summary>
		/// Some list of strings.
		/// </summary>
		public string[] Paths { get; set; } = new string[] { "%FARHOME%" };

		/// <summary>
		/// Regex created by <see cref="Validate"/> from <see cref="Regex"/>.
		/// </summary>
		[XmlIgnore]
		public Regex Regex2 { get; set; }

		/// <summary>
		/// Example of validating and completing data.
		/// </summary>
		/// <remarks>
		/// This interface method is called after deserializing or creating default data.
		/// This sample creates <see cref="Regex2"/> from its pattern <see cref="Regex"/>.
		/// </remarks>
		public void Validate()
		{
			try
			{
				// try to create the regex from its pattern
				Regex2 = new Regex(Regex);
			}
			catch (Exception ex)
			{
				// throw amended exception with invalid data details
				throw new Exception($"{nameof(Regex)}: {ex.Message}", ex);
			}
		}
	}

	/// <summary>
	/// Example of migrating data from the old XML.
	/// </summary>
	/// <remarks>
	/// In order to test, remove the XML element <c>Version</c> or set it to 0.
	/// </remarks>
	protected override bool UpdateData(Data data)
	{
		// do noting if the saved version is current or future
		if (data.SavedVersion >= CurrentVersion)
			return false;

		// get the old XML
		var xml = new XmlDocument();
		xml.Load(FileName);

		// get old data
		var nodeVersion = xml.SelectSingleNode("Data/Version");

		// update new data
		data.Name = $"Updated from version '{nodeVersion?.InnerText}'.";

		// tell to save new data (the current version is saved, too)
		return true;
	}
}
