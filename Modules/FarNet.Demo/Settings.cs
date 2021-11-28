using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace FarNet.Demo
{
	/// <summary>
	/// To define roaming browsable settings, just implement <see cref="ModuleSettings{T}"/>.
	/// </summary>
	/// <remarks>
	/// The type name is used as the settings file name and as the display name in the settings menu.
	/// Specify the template parameter type <c>Settings.Data</c> which defines the stored settings.
	/// </remarks>
	/// <seealso cref="Workings"/>
	public class Settings : ModuleSettings<Settings.Data>
	{
		/// <summary>
		/// This cached global settings is known to FarNet due to the conventional name <c>Default</c>.
		/// When you edit settings via the FarNet settings menu this instance is updated automatically.
		/// </summary>
		public static Settings Default { get; } = new Settings();

		/// <summary>
		/// This type defines the settings.
		/// </summary>
		/// <remarks>
		/// Use the required attribute <see cref="SerializableAttribute"/> and optional <see cref="XmlRootAttribute"/>.
		/// Optionally implement the <see cref="IValidate"/> interface with the method <see cref="Validate"/>.
		/// This method is automatically called after deserializing.
		/// </remarks>
		[Serializable]
		[XmlRoot("Data")]
		public class Data : IValidate
		{
			/// <summary>
			/// Some simple string.
			/// </summary>
			public string Name { get; set; } = "John Doe";

			/// <summary>
			/// Some number.
			/// </summary>
			public int Age { get; set; } = 42;

			/// <summary>
			/// Some list of strings.
			/// </summary>
			public string[] Paths { get; set; } = new string[] { Environment.GetEnvironmentVariable("FARHOME") };

			/// <summary>
			/// CDATA with empty default.
			/// </summary>
			public XmlCData Memo { get; set; }

			/// <summary>
			/// CDATA with default text.
			/// </summary>
			public XmlCData Regex { get; set; } = new XmlCData("([<>&]+)");

			/// <summary>
			/// Internal regex created by <see cref="Validate"/> from <see cref="Regex"/>.
			/// </summary>
			internal Regex Regex2 { get; set; }

			/// <summary>
			/// This interface method is automatically called after deserializing.
			/// </summary>
			/// <remarks>
			/// This sample creates the cached <see cref="Regex2"/> from its pattern <see cref="Regex"/>.
			/// </remarks>
			public void Validate()
			{
				try
				{
					// try to create the regex from its pattern
					Regex2 = new Regex(Regex.Value);
				}
				catch (Exception ex)
				{
					// throw another exception with the data name included in the message.
					throw new Exception($"{nameof(Regex)}: {ex.Message}", ex);
				}
			}
		}
	}
}
