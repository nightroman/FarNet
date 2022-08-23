
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
		if (error == null)
			throw new ArgumentNullException(nameof(error));

		//?? _090901_055134 Regex is used to fix bad PS V1 strings; check V2
		var re = new Regex("[\r\n]+");

		writer.Write(error.GetType().Name);
		writer.WriteLine(":");
		writer.WriteLine(re.Replace(error.Message, Environment.NewLine));

		// get an error record
		if (error.GetType().FullName.StartsWith("System.Management.Automation.", StringComparison.Ordinal))
		{
			object errorRecord = GetPropertyValue(error, "ErrorRecord");
			if (errorRecord != null)
			{
				// process the error record
				object ii = GetPropertyValue(errorRecord, "InvocationInfo");
				if (ii != null)
				{
					object pm = GetPropertyValue(ii, "PositionMessage");
					if (pm != null)
						//?? 090517 Added Trim(), because a position message starts with an empty line
						writer.WriteLine(re.Replace(pm.ToString().Trim(), Environment.NewLine));
				}
			}
		}

		if (error.InnerException != null)
		{
			writer.WriteLine();
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
		// no job?
		if (null == error || !Source.Switch.ShouldTrace(TraceEventType.Error))
			return;

		// find the last dot
		string type = error.GetType().FullName;
		int i = type.LastIndexOf('.');

		// system error: trace as error
		if (i >= 0 && type.Substring(0, i) == "System")
		{
			Source.TraceEvent(TraceEventType.Error, 0, FormatException(error));
		}
		// other error: trace as warning
		else if (Source.Switch.ShouldTrace(TraceEventType.Warning))
		{
			Source.TraceEvent(TraceEventType.Warning, 0, FormatException(error));
		}
	}

	// gets property value or null
	static object GetPropertyValue(object obj, string name)
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
