using System.Globalization;

namespace PowerShellFar;

/// <summary>
/// Helper methods.
/// </summary>
static class Kit
{
	/// <summary>
	/// Converts with culture.
	/// </summary>
	public static string ToString<T>(T value) where T : IConvertible //! IConvertible is not CLS-compliant
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Converts with culture.
	/// </summary>
	public static string ToString(DateTime value, string format)
	{
		return value.ToString(format, CultureInfo.CurrentCulture);
	}

	// Compares strings OrdinalIgnoreCase.
	public static bool Equals(string? strA, string? strB)
	{
		return string.Equals(strA, strB, StringComparison.OrdinalIgnoreCase);
	}
}
