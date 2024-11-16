
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Diagnostics;
using System.IO;

namespace FarNet;

/// <summary>
/// INTERNAL
/// </summary>
public static class Log
{
	/// <summary>
	/// INTERNAL
	/// </summary>
	public static TraceSource Source { get; } = new("FarNet", SourceLevels.Warning);

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
		ArgumentNullException.ThrowIfNull(error);

		writer.Write(error.GetType().FullName);
		writer.Write(": ");
		writer.WriteLine(error.Message);

		// get an error record
		if (error.GetType().FullName!.StartsWith("System.Management.Automation."))
		{
			if (GetPropertyValue(error, "ErrorRecord") is { } errorRecord)
			{
				// process the error record
				if (GetPropertyValue(errorRecord, "InvocationInfo") is { } ii)
				{
					if (GetPropertyValue(ii, "PositionMessage") is { } pm)
						writer.WriteLine(pm.ToString());
				}
			}
		}

		if (error.InnerException is { })
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
		if (error is { } && Source.Switch.ShouldTrace(TraceEventType.Error))
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
