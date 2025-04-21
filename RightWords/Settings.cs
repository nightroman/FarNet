using FarNet;
using System.Text.RegularExpressions;

namespace RightWords;

public sealed class Settings : ModuleSettings<Settings.Data>
{
	internal const string ModuleName = "RightWords";
	internal const string UserFile = "RightWords.dic";

	public static Settings Default { get; } = new Settings();

	public class Data : IValidate
	{
		public ConsoleColor HighlightingForegroundColor { get; set; } = ConsoleColor.Black;

		public ConsoleColor HighlightingBackgroundColor { get; set; } = ConsoleColor.Yellow;

		public int MaximumLineLength { get; set; } = 0;

		public XmlCData WordRegex { get; set; } = @"[\p{Lu}\p{Ll}]\p{Ll}+";

		public XmlCData SkipRegex { get; set; }

		public XmlCData RemoveRegex { get; set; }

		public string[] Prefixes { get; set; } = ["sub", "un"];

		public string? UserDictionaryDirectory { get; set; }

		internal Regex WordRegex2 { get; private set; } = null!;
		internal Regex? SkipRegex2 { get; private set; }
		internal Regex? RemoveRegex2 { get; private set; }
		public void Validate()
		{
			if (string.IsNullOrWhiteSpace(WordRegex))
				throw new ModuleException("WordRegex cannot be empty.");

			try { WordRegex2 = new Regex(WordRegex.Value.Trim()); }
			catch (Exception ex) { throw new ModuleException($"WordRegex: {ex.Message}"); }

			if (!string.IsNullOrWhiteSpace(SkipRegex))
			{
				try { SkipRegex2 = new Regex(SkipRegex.Value.Trim()); }
				catch (Exception ex) { throw new ModuleException($"SkipRegex: {ex.Message}"); }
			}

			if (!string.IsNullOrWhiteSpace(RemoveRegex))
			{
				try { RemoveRegex2 = new Regex(RemoveRegex.Value.Trim()); }
				catch (Exception ex) { throw new ModuleException($"RemoveRegex: {ex.Message}"); }
			}
		}
	}
}
