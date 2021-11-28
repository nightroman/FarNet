
// FarNet module RightControl
// Copyright (c) Roman Kuzmin

using System;
using System.Text.RegularExpressions;

namespace FarNet.RightControl
{
	public sealed class Settings : ModuleSettings<Settings.Data>
	{
		public static Settings Default { get; } = new Settings();

		[Serializable]
		public class Data : IValidate
		{
			public XmlCData RegexLeft { get; set; } = new XmlCData(@"(?x: ^ | $ | (?<=\b|\s)\S )");

			public XmlCData RegexRight { get; set; } = new XmlCData(@"(?x: ^ | $ | (?<=\b|\s)\S )");

			internal Regex RegexLeft2 { get; private set; }
			internal Regex RegexRight2 { get; private set; }
			public void Validate()
			{
				try
				{
					RegexLeft2 = new Regex(RegexLeft.Value);
				}
				catch (Exception ex)
				{
					throw new ModuleException($"RegexLeft: {ex.Message}", ex);
				}

				try
				{
					RegexRight2 = new Regex(RegexRight.Value);
				}
				catch (Exception ex)
				{
					throw new ModuleException($"RegexRight: {ex.Message}", ex);
				}
			}
		}
	}
}
