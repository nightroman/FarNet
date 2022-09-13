
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Text.RegularExpressions;

namespace PowerShellFar;

abstract class OutputWriter
{
	#region https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/FormatAndOutput/common/StringDecorated.cs
	// graphics/color mode ESC[1;2;...m
	const string GraphicsRegex = @"(\x1b\[\d+(;\d+)*m)";
	// CSI escape sequences
	const string CsiRegex = @"(\x1b\[\?\d+[hl])";
	// Hyperlink escape sequences. Note: '.*?' makes '.*' do non-greedy match.
	const string HyperlinkRegex = @"(\x1b\]8;;.*?\x1b\\)";
	// replace regex with .NET 6 API once available
	static readonly Regex AnsiRegex = new($"{GraphicsRegex}|{CsiRegex}|{HyperlinkRegex}", RegexOptions.Compiled);
	#endregion

	protected static string RemoveOutputRendering(string s)
	{
		return AnsiRegex.Replace(s, string.Empty);
	}

	public OutputWriter? Next { get; set; }
	public abstract void Write(string value);
	public abstract void WriteLine();
	public abstract void WriteLine(string value);
	public abstract void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
	public abstract void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
	public abstract void WriteDebugLine(string message);
	public abstract void WriteErrorLine(string value);
	public abstract void WriteVerboseLine(string message);
	public abstract void WriteWarningLine(string message);
}
