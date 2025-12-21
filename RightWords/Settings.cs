using FarNet;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RightWords;

public sealed class Settings : ModuleSettings<Settings.Data>
{
	internal const string ModuleName = "RightWords";
	internal const string UserFile = "RightWords.dic";

	public static Settings Default { get; } = new();

	public class Data : IValidatableObject
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

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			(WordRegex2, var err) = Validators.Regex(WordRegex, nameof(WordRegex));
			if (err is { })
				return [err];

			if (!string.IsNullOrWhiteSpace(SkipRegex))
			{
				(SkipRegex2, err) = Validators.Regex(SkipRegex.Value, nameof(SkipRegex));
				if (err is { })
					return [err];
			}

			if (!string.IsNullOrWhiteSpace(RemoveRegex))
			{
				(RemoveRegex2, err) = Validators.Regex(RemoveRegex.Value, nameof(RemoveRegex));
				if (err is { })
					return [err];
			}

			return [];
		}
	}
}
