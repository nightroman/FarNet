
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FarNet;

/// <summary>
/// INTERNAL
/// </summary>
public static class Log
{
	/// <summary>
	/// INTERNAL
	/// </summary>
	public static TraceSource Source { get; } = new("FarNet", DefaultSourceLevels());
	static SourceLevels DefaultSourceLevels()
	{
		var str = Environment.GetEnvironmentVariable("FarNet:TraceLevel");
		return str is not null && Enum.TryParse(str, out SourceLevels value) ? value : SourceLevels.Warning;
	}

	/// <summary>
	/// INTERNAL, consider using variant with writer
	/// </summary>
	/// <param name="error">.</param>
	public static string FormatException(Exception error)
	{
		var writer = new StringWriter();
		FormatException(writer, error);
		return writer.ToString();
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="writer">.</param>
	/// <param name="error">.</param>
	public static void FormatException(TextWriter writer, Exception error)
	{
		if (error is null)
			throw new ArgumentNullException(nameof(error));

		writer.Write(error.GetType().FullName);
		writer.Write(": ");
		writer.WriteLine(error.Message);

		// get an error record
		if (error.GetType().FullName!.StartsWith("System.Management.Automation."))
		{
			var errorRecord = GetPropertyValue(error, "ErrorRecord");
			if (errorRecord != null)
			{
				// process the error record
				var ii = GetPropertyValue(errorRecord, "InvocationInfo");
				if (ii != null)
				{
					var pm = GetPropertyValue(ii, "PositionMessage");
					if (pm != null)
						writer.WriteLine(pm.ToString());
				}
			}
		}

		if (error.InnerException != null)
		{
			writer.WriteLine();
			writer.WriteLine("InnerException:");
			FormatException(writer, error.InnerException);
		}
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="error">The error message.</param>
	public static void TraceError(string error)
	{
		Source.TraceEvent(TraceEventType.Error, 0, error);
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="error">.</param>
	public static void TraceException(Exception error)
	{
		if (error != null && Source.Switch.ShouldTrace(TraceEventType.Error))
			Source.TraceEvent(TraceEventType.Error, 0, FormatException(error));
	}

	// gets property value or null
	static object? GetPropertyValue(object obj, string name)
	{
		try
		{
			var meta = obj.GetType().GetProperty(name);
			return meta?.GetValue(obj);
		}
		catch
		{
			return null;
		}
	}
}
