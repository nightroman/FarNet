using FarNet;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RightControl;

public sealed class Settings : ModuleSettings<Settings.Data>
{
	public static Settings Default { get; } = new();

	public class Data : IValidatableObject
	{
		public XmlCData RegexLeft { get; set; } = @"(?x: ^ | $ | (?<=\b|\s)\S )";

		public XmlCData RegexRight { get; set; } = @"(?x: ^ | $ | (?<=\b|\s)\S )";

		internal Regex RegexLeft2 { get; private set; } = null!;

		internal Regex RegexRight2 { get; private set; } = null!;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			(RegexLeft2, var err) = Validators.Regex(RegexLeft, nameof(RegexLeft));
			if (err is { })
				return [err];

			(RegexRight2, err) = Validators.Regex(RegexRight, nameof(RegexRight));
			if (err is { })
				return [err];

			return [];
		}
	}
}
