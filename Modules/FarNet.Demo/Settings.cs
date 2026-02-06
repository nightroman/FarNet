using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
	/// Optionally implement <see cref="IValidatableObject"/> for validation.
	/// </remarks>
	public class Data : IValidatableObject
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
		/// Number using range validation.
		/// </summary>
		[Range(0, 200)]
		public int Age { get; set; } = 42;

		/// <summary>
		/// Enum using data type validation.
		/// </summary>
		[EnumDataType(typeof(ConsoleColor))]
		public string Color { get; set; } = "Black";

		/// <summary>
		/// CDATA with empty text.
		/// </summary>
		public XmlCData Memo { get; set; }

		/// <summary>
		/// CDATA with some text.
		/// </summary>
		public XmlCData Regex { get; set; } = "([<>&]+)";

		/// <summary>
		/// List of strings with length validation.
		/// </summary>
		[Length(1, 10)]
		public string[] Paths { get; set; } = ["%FARHOME%"];

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
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			List<ValidationResult> r = [];
			try
			{
				// try to create the regex from its pattern
				Regex2 = new Regex(Regex);
			}
			catch (Exception ex)
			{
				r.Add(new($"Invalid regex pattern: {ex.Message}", [nameof(Regex)]));
			}
			return r;
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
