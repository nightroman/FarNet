using System.Management.Automation;

namespace My;

/// <summary>
/// My System.IO.Path extensions.
/// </summary>
/// <remarks>
/// System.IO.Path is not OK due to invalid file system chars that are valid for other providers.
/// </remarks>
static class PathEx
{
	public static bool IsPSFile(string fileName)
	{
		return
			fileName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) ||
			fileName.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase) ||
			fileName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Does a string looks like a file system path?
	/// </summary>
	public static bool IsFSPath(string name)
	{
		return name.StartsWith("\\\\") || (name.Length > 3 && name[1] == ':');
	}

	public static string Combine(string path, string file)
	{
		// no path or special fake path, e.g. in object panel
		if (path is null || path == "*")
			return file;

		if (path.EndsWith('\\'))
			return path + file;

		// 090824
		if (path.EndsWith("::"))
			return path + file;
		else
			return path + "\\" + file;
	}

	public static string GetFileName(string path)
	{
		int i = path.LastIndexOf('\\');
		if (i < 0)
			return path;

		return path[(i + 1)..];
	}

	public static string GetDirectoryName(string path)
	{
		int i = path.LastIndexOf('\\');
		if (i < 0)
			return string.Empty;

		return path[..i];
	}

	/// <summary>
	/// Tries to recognize an existing file path by an object.
	/// </summary>
	/// <param name="value">Any object, e.g. FileInfo, String.</param>
	/// <returns>Existing file path or null.</returns>
	public static string? TryGetFilePath(object? value) //_091202_073429
	{
		var fileInfo = PowerShellFar.Cast<FileInfo>.From(value);
		if (fileInfo is { })
			return fileInfo.FullName;

		{
			if (LanguagePrimitives.TryConvertTo(value, out string path))
			{
				// looks like a full path
				if (path.Length > 3 && path.AsSpan(1, 2).SequenceEqual(":\\") || path.StartsWith("\\\\"))
				{
					if (File.Exists(path))
						return path;
				}
			}
		}
		{

			var pso = PSObject.AsPSObject(value);
			var path = pso.Properties["Path"]?.Value as string;
			if (File.Exists(path))
				return path;
		}

		return null;
	}

	/// <summary>
	/// Tries to recognize an existing file path by an object.
	/// </summary>
	/// <param name="value">Any object, e.g. DirectoryInfo, String.</param>
	/// <returns>Existing directory path or null.</returns>
	public static string? TryGetDirectoryPath(object value)
	{
		var directoryInfo = PowerShellFar.Cast<DirectoryInfo>.From(value);
		if (directoryInfo is { })
			return directoryInfo.FullName;

		if (LanguagePrimitives.TryConvertTo(value, out string path))
		{
			// looks like a full path
			if (path.Length > 3 && path.AsSpan(1, 2).SequenceEqual(":\\") || path.StartsWith("\\\\"))
			{
				if (Directory.Exists(path))
					return path;
			}
		}

		return null;
	}
}
