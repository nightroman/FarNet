using FarNet;

namespace PowerShellFar;

sealed class ConsoleOutputWriter : AbcOutputWriter
{
	Func<string>? _getEcho;

	public ConsoleOutputWriter()
	{
	}

	public ConsoleOutputWriter(Func<string>? getEcho)
	{
		_getEcho = getEcho;
	}

	void Writing()
	{
		// use and null echo
		if (_getEcho is { })
		{
			if (Transcript.Writer is { } writer)
			{
				var echo = _getEcho() + Environment.NewLine;
				writer.WriteEcho(echo);
			}
			_getEcho = null;
		}
	}

	public override void Write(string value)
	{
		if (Settings.Default.RemoveOutputRendering)
			value = RemoveOutputRendering(value);

		Writing();
		Far.Api.UI.Write(value);
		Transcript.Writer?.Write(value);
	}

	public override void WriteLine()
	{
		Writing();
		Far.Api.UI.WriteLine();
		Transcript.Writer?.WriteLine();
	}

	public override void WriteLine(string value)
	{
		if (Settings.Default.RemoveOutputRendering)
			value = RemoveOutputRendering(value);

		Writing();
		Far.Api.UI.WriteLine(value);
		Transcript.Writer?.WriteLine(value);
	}

	public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
	{
		Writing();
		Far.Api.UI.Write(value, foregroundColor, backgroundColor);
		Transcript.Writer?.Write(value);
	}

	public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
	{
		Writing();
		Far.Api.UI.WriteLine(value, foregroundColor, backgroundColor);
		Transcript.Writer?.WriteLine(value);
	}

	public override void WriteDebugLine(string message)
	{
		Writing();
		Far.Api.UI.WriteLine("DEBUG: " + message, Settings.Default.DebugForegroundColor);
		Transcript.Writer?.WriteDebugLine(message);
	}

	public override void WriteErrorLine(string value)
	{
		Writing();
		Far.Api.UI.WriteLine(value, Settings.Default.ErrorForegroundColor);
		Transcript.Writer?.WriteErrorLine(value);
	}

	public override void WriteVerboseLine(string message)
	{
		Writing();
		Far.Api.UI.WriteLine("VERBOSE: " + message, Settings.Default.VerboseForegroundColor);
		Transcript.Writer?.WriteVerboseLine(message);
	}

	public override void WriteWarningLine(string message)
	{
		Writing();
		Far.Api.UI.WriteLine("WARNING: " + message, Settings.Default.WarningForegroundColor);
		Transcript.Writer?.WriteWarningLine(message);
	}
}
