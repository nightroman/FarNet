
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

using System;
using System.Text.RegularExpressions;

namespace FarNet.RightWords;

public sealed class Settings : ModuleSettings<Settings.Data>
{
	internal const string ModuleName = "RightWords";
	internal const string UserFile = "RightWords.dic";

	public static Settings Default { get; } = new Settings();

	public class Data : IValidate
	{
		public XmlCData WordRegex { get; set; } = @"[\p{Lu}\p{Ll}]\p{Ll}+";

		public XmlCData SkipRegex { get; set; }

		public ConsoleColor HighlightingForegroundColor { get; set; } = ConsoleColor.Black;

		public ConsoleColor HighlightingBackgroundColor { get; set; } = ConsoleColor.Yellow;

		public string UserDictionaryDirectory { get; set; }

		public int MaximumLineLength { get; set; } = 0;

		internal Regex WordRegex2 { get; private set; }
		internal Regex SkipRegex2 { get; private set; }
		public void Validate()
		{
			if (string.IsNullOrWhiteSpace(WordRegex))
				throw new ModuleException("WordRegex cannot be empty.");

			try
			{
				WordRegex2 = new Regex(WordRegex);
			}
			catch (ArgumentException ex)
			{
				throw new ModuleException($"WordRegex: {ex.Message}");
			}

			if (!string.IsNullOrWhiteSpace(SkipRegex))
			{
				try
				{
					SkipRegex2 = new Regex(SkipRegex);
				}
				catch (ArgumentException ex)
				{
					throw new ModuleException($"SkipRegex: {ex.Message}");
				}
			}
		}
	}
}
