
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace PowerShellFar
{
	abstract class TextOutputWriter : OutputWriter
	{
		WriteMode _mode;
		/// <summary>
		/// 1 of 3 actual writers.
		/// </summary>
		protected abstract void Append(string value);
		/// <summary>
		/// 2 of 3 actual writers.
		/// </summary>
		protected abstract void AppendLine();
		/// <summary>
		/// 3 of 3 actual writers.
		/// </summary>
		protected abstract void AppendLine(string value);
		public sealed override void Write(string value)
		{
			value = RemoveAnsi(value);

			_mode = WriteMode.None;
			Append(value);
		}
		public sealed override void WriteLine()
		{
			_mode = WriteMode.None;
			AppendLine();
		}
		public sealed override void WriteLine(string value)
		{
			value = RemoveAnsi(value);

			_mode = WriteMode.None;
			AppendLine(value);
		}
		public sealed override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			_mode = WriteMode.None;
			Append(value);
		}
		public sealed override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			_mode = WriteMode.None;
			AppendLine(value);
		}
		public sealed override void WriteDebugLine(string message)
		{
			if (_mode != WriteMode.Debug)
			{
				_mode = WriteMode.Debug;
				AppendLine("DEBUG:");
			}
			AppendLine(message);
		}
		public sealed override void WriteErrorLine(string value)
		{
			if (_mode != WriteMode.Error)
			{
				_mode = WriteMode.Error;
				AppendLine("ERROR:");
			}
			AppendLine(value);
		}
		public sealed override void WriteVerboseLine(string message)
		{
			if (_mode != WriteMode.Verbose)
			{
				_mode = WriteMode.Verbose;
				AppendLine("VERBOSE:");
			}
			AppendLine(message);
		}
		public sealed override void WriteWarningLine(string message)
		{
			if (_mode != WriteMode.Warning)
			{
				_mode = WriteMode.Warning;
				AppendLine("WARNING:");
			}
			AppendLine(message);
		}
	}
}
