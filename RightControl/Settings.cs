using FarNet;
using System;
using System.Text.RegularExpressions;

namespace RightControl;

public sealed class Settings : ModuleSettings<Settings.Data>
{
	public static Settings Default { get; } = new Settings();

	public class Data : IValidate
	{
		public XmlCData RegexLeft { get; set; } = @"(?x: ^ | $ | (?<=\b|\s)\S )";

		public XmlCData RegexRight { get; set; } = @"(?x: ^ | $ | (?<=\b|\s)\S )";

		internal Regex RegexLeft2 { get; private set; } = null!;

		internal Regex RegexRight2 { get; private set; } = null!;

		public void Validate()
		{
			if (string.IsNullOrWhiteSpace(RegexLeft))
				throw new ModuleException("RegexLeft cannot be empty.");

			if (string.IsNullOrWhiteSpace(RegexRight))
				throw new ModuleException("RegexRight cannot be empty.");

			try { RegexLeft2 = new Regex(RegexLeft.Value.Trim()); }
			catch (Exception ex) { throw new ModuleException($"RegexLeft: {ex.Message}"); }

			try { RegexRight2 = new Regex(RegexRight.Value.Trim()); }
			catch (Exception ex) { throw new ModuleException($"RegexRight: {ex.Message}"); }
		}
	}
}
