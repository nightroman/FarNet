/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	abstract class OutputWriter
	{
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

	sealed class ConsoleOutputWriter : OutputWriter
	{
		public override void Write(string value)
		{
			Far.Net.Write(value);
		}

		public override void WriteLine()
		{
			Far.Net.Write("\r\n");
		}

		public override void WriteLine(string value)
		{
			Far.Net.Write(value + "\r\n");
		}

		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Far.Net.Write(value, foregroundColor, backgroundColor);
		}

		public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Far.Net.Write(value + "\r\n", foregroundColor, backgroundColor);
		}

		public override void WriteDebugLine(string message)
		{
			Far.Net.Write("DEBUG: " + message + "\r\n", ConsoleColor.Yellow);
		}

		public override void WriteErrorLine(string value)
		{
			Far.Net.Write(value + "\r\n", ConsoleColor.Red);
		}

		public override void WriteVerboseLine(string message)
		{
			Far.Net.Write("VERBOSE: " + message + "\r\n", ConsoleColor.DarkGray);
		}

		public override void WriteWarningLine(string message)
		{
			Far.Net.Write("WARNING: " + message + "\r\n", ConsoleColor.Yellow);
		}
	}

	abstract class TextOutputWriter : OutputWriter
	{
		WriteMode _mode;

		/// <summary>
		/// 1st of 3 actual writers.
		/// </summary>
		protected abstract void Append(string value);

		/// <summary>
		/// 2nd of 3 actual writers.
		/// </summary>
		protected abstract void AppendLine();

		/// <summary>
		/// 3rd of 3 actual writers.
		/// </summary>
		protected abstract void AppendLine(string value);

		public sealed override void Write(string value)
		{
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

	sealed class StreamOutputWriter : TextOutputWriter
	{
		StreamWriter _writer;
		
		public StreamOutputWriter(StreamWriter writer)
		{
			_writer = writer;
		}

		protected override void Append(string value)
		{
			_writer.Write(value);
		}

		protected override void AppendLine()
		{
			_writer.WriteLine();
		}

		protected override void AppendLine(string value)
		{
			_writer.WriteLine(value);
		}
	}

	/// <summary>
	/// Trivial editor writer, for example asynchronous.
	/// </summary>
	class EditorOutputWriter1 : TextOutputWriter
	{
		/// <summary>
		/// The editor.
		/// </summary>
		protected IEditor Editor { get; private set; }

		/// <summary>
		/// Write call count.
		/// </summary>
		internal int WriteCount { get; private set; }

		public EditorOutputWriter1(IEditor editor)
		{
			Editor = editor;
		}

		protected override void Append(string value)
		{
			// start
			if (++WriteCount == 1)
				Editor.InsertText("\r<=\r");

			// insert
			Editor.InsertText(value);
		}

		protected override void AppendLine()
		{
			// start
			if (++WriteCount == 1)
				Editor.InsertText("\r<=\r");
			else
				Editor.InsertLine();
		}

		protected override void AppendLine(string value)
		{
			// start
			if (++WriteCount == 1)
				Editor.InsertText("\r<=\r");

			// insert trimmed
			Editor.InsertText(value.TrimEnd() + "\r");
		}
	}

	/// <summary>
	/// Advanced editor synchronous writer.
	/// </summary>
	sealed class EditorOutputWriter2 : EditorOutputWriter1
	{
		Stopwatch _stopwatch = Stopwatch.StartNew();

		public EditorOutputWriter2(IEditor editor) : base(editor) { }

		void Redraw()
		{
			// max 25 redraw per second
			if (_stopwatch.ElapsedMilliseconds > 40)
			{
				Editor.Redraw();
				_stopwatch = Stopwatch.StartNew();
			}
		}

		protected override void Append(string value)
		{
			base.Append(value);
			Redraw();
		}

		protected override void AppendLine()
		{
			base.AppendLine();
			Redraw();
		}

		protected override void AppendLine(string value)
		{
			base.AppendLine(value);
			Redraw();
		}
	}

	sealed class StringOutputWriter : TextOutputWriter
	{
		StringBuilder _output = new StringBuilder();

		public StringOutputWriter()
		{ }

		/// <summary>
		/// Output builder
		/// </summary>
		internal StringBuilder Output
		{
			get { return _output; }
		}

		protected override void Append(string value)
		{
			_output.Append(value);
		}

		protected override void AppendLine()
		{
			_output.AppendLine();
		}

		protected override void AppendLine(string value)
		{
			_output.AppendLine(value);
		}
	}

	sealed class ExternalOutputWriter : TextOutputWriter, IDisposable
	{
		// Output file name
		string FileName;

		// Outer process
		Process Process;

		// Output writer
		StreamWriter Writer;

		void Open()
		{
			// new writer
			if (Writer == null)
			{
				if (FileName == null)
					FileName = Path.GetTempFileName();

				Writer = new StreamWriter(FileName, false, Encoding.Unicode);
				Writer.AutoFlush = true;
			}

			//! start after opening a writer when BOM is already written
			StartViewer();
		}

		void StartViewer()
		{
			if (Process == null || Process.HasExited)
			{
				string externalViewerFileName = A.Psf.Settings.ExternalViewerFileName;
				string externalViewerArguments;

				// try user defined external viewer
				if (!string.IsNullOrEmpty(externalViewerFileName))
				{
					externalViewerArguments = Invariant.Format(A.Psf.Settings.ExternalViewerArguments, FileName);
					try
					{
						Process = My.ProcessEx.Start(externalViewerFileName, externalViewerArguments);
						Process.EnableRaisingEvents = true;
						Process.Exited += OnExited;
					}
					catch (Exception)
					{
						Far.Net.Message(
							"Cannot start the external viewer, default viewer will be used.\nYour settings:\nExternalViewerFileName: " + externalViewerFileName + "\nExternalViewerArguments: " + A.Psf.Settings.ExternalViewerArguments,
							Res.Me, MsgOptions.LeftAligned | MsgOptions.Warning);
					}
				}

				// use default external viewer
				if (Process == null || Process.HasExited)
				{
					externalViewerFileName = Process.GetCurrentProcess().MainModule.FileName;
					externalViewerArguments = "/m /p /v \"" + FileName + "\"";

					Process = My.ProcessEx.Start(externalViewerFileName, externalViewerArguments);
					Process.EnableRaisingEvents = true;
					Process.Exited += OnExited;
				}
			}
		}

		// Closes writing.
		// *) Do not delete the file: case: viewer is starting and has not yet opened the file => output won't be shown.
		// *) As a result: If Far is closed but external viewers are not yet then their files are not deleted.
		public void Dispose()
		{
			if (Writer != null)
			{
				Writer.Close();
				Writer = null;
			}
			GC.SuppressFinalize(this); // CA1816
		}

		// Try to delete the file: ignore IO errors, the file still may be in use.
		void OnExited(object sender, EventArgs e)
		{
			if (FileName != null && File.Exists(FileName))
			{
				try
				{
					File.Delete(FileName);
				}
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
			}
		}

		protected override void Append(string value)
		{
			Open();
			Writer.Write(value);
		}

		protected override void AppendLine()
		{
			Open();
			Writer.WriteLine();
		}

		protected override void AppendLine(string value)
		{
			Open();
			Writer.WriteLine(value);
		}
	}
}
