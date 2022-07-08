
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Text.RegularExpressions;

namespace PowerShellFar
{
	abstract class OutputWriter
	{
		//https://stackoverflow.com/q/14693701/323582
		static readonly Regex s_regexAnsi = new(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])");

		protected static string RemoveAnsi(string s) //rk-0
		{
			return s_regexAnsi.Replace(s, string.Empty);
		}

		public OutputWriter Next { get; set; }
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
}
