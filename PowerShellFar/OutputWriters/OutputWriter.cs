
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace PowerShellFar
{
	abstract class OutputWriter
	{
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
