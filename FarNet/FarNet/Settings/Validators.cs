using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FarNet;

/// <summary>
/// Common validation helpers.
/// </summary>
public static class Validators
{
	/// <summary>
	/// Validates and creates a regular expression.
	/// </summary>
	/// <param name="value">Regex pattern, to be trimmed.</param>
	/// <param name="field">Field name.</param>
	/// <returns>Result or error.</returns>
	public static (Regex, ValidationResult?) Regex(string value, string field)
	{
		if (string.IsNullOrWhiteSpace(value))
			return (null!, new($"Regex requires not empty pattern.", [field]));

		try
		{
			return (new Regex(value.Trim()), null);
		}
		catch (ArgumentException ex)
		{
			return (null!, new(ex.Message, [field]));
		}
	}
}
