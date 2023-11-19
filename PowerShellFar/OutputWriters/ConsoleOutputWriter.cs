
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using FarNet;

namespace PowerShellFar;

sealed class ConsoleOutputWriter : OutputWriter
{
	string? _echo;

	public ConsoleOutputWriter()
	{
	}

	public ConsoleOutputWriter(string? echo)
	{
		_echo = echo;
	}

	void Writing()
	{
		// write and drop echo
		if (_echo != null)
		{
			var echo2 = _echo + Environment.NewLine;
			Far.Api.UI.Write(echo2, Settings.Default.CommandForegroundColor);
			_echo = null;

			A.Psf.Transcript?.WriteLine(Environment.NewLine + echo2);
		}
	}

	public override void Write(string value)
	{
		if (Settings.Default.RemoveOutputRendering)
			value = RemoveOutputRendering(value);

		Writing();
		Far.Api.UI.Write(value);
		A.Psf.Transcript?.Write(value);
	}

	public override void WriteLine()
	{
		Writing();
		Far.Api.UI.WriteLine();
		A.Psf.Transcript?.WriteLine();
	}

	public override void WriteLine(string value)
	{
		if (Settings.Default.RemoveOutputRendering)
			value = RemoveOutputRendering(value);

		Writing();
		Far.Api.UI.WriteLine(value);
		A.Psf.Transcript?.WriteLine(value);
	}

	public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
	{
		Writing();
		Far.Api.UI.Write(value, foregroundColor, backgroundColor);
		A.Psf.Transcript?.Write(value);
	}

	public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
	{
		Writing();
		Far.Api.UI.WriteLine(value, foregroundColor, backgroundColor);
		A.Psf.Transcript?.WriteLine(value);
	}

	public override void WriteDebugLine(string message)
	{
		Writing();
		Far.Api.UI.WriteLine("DEBUG: " + message, Settings.Default.DebugForegroundColor);
		A.Psf.Transcript?.WriteDebugLine(message);
	}

	public override void WriteErrorLine(string value)
	{
		Writing();
		Far.Api.UI.WriteLine(value, Settings.Default.ErrorForegroundColor);
		A.Psf.Transcript?.WriteErrorLine(value);
	}

	public override void WriteVerboseLine(string message)
	{
		Writing();
		Far.Api.UI.WriteLine("VERBOSE: " + message, Settings.Default.VerboseForegroundColor);
		A.Psf.Transcript?.WriteVerboseLine(message);
	}

	public override void WriteWarningLine(string message)
	{
		Writing();
		Far.Api.UI.WriteLine("WARNING: " + message, Settings.Default.WarningForegroundColor);
		A.Psf.Transcript?.WriteWarningLine(message);
	}
}
