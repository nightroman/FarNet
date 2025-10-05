using System.Diagnostics;

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
		if (error is { })
			Source.TraceData(TraceEventType.Error, 0, error);
	}
}
