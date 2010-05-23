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
		string _command;

		public ConsoleOutputWriter() { }

		public ConsoleOutputWriter(string command)
		{
			_command = command;
		}

		void Writing()
		{
			// echo the command and drop it
			if (_command != null)
			{
				string header = string.Concat(Entry.CommandInvoke1.Prefix, ":", _command, "\r\n");
				Far.Net.Write(header, A.Psf.Settings.CommandForegroundColor);
				_command = null;

				A.Psf.Transcript.WriteLine("\r\n" + header);
			}
		}

		public override void Write(string value)
		{
			Writing();
			Far.Net.Write(value);
			A.Psf.Transcript.Write(value);
		}

		public override void WriteLine()
		{
			Writing();
			Far.Net.Write("\r\n");
			A.Psf.Transcript.WriteLine();
		}

		public override void WriteLine(string value)
		{
			Writing();
			Far.Net.Write(value + "\r\n");
			A.Psf.Transcript.WriteLine(value);
		}

		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Writing();
			Far.Net.Write(value, foregroundColor, backgroundColor);
			A.Psf.Transcript.Write(value);
		}

		public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Writing();
			Far.Net.Write(value + "\r\n", foregroundColor, backgroundColor);
			A.Psf.Transcript.WriteLine(value);
		}

		public override void WriteDebugLine(string message)
		{
			Writing();
			Far.Net.Write("DEBUG: " + message + "\r\n", A.Psf.Settings.DebugForegroundColor);
			A.Psf.Transcript.WriteDebugLine(message);
		}

		public override void WriteErrorLine(string value)
		{
			Writing();
			Far.Net.Write(value + "\r\n", A.Psf.Settings.ErrorForegroundColor);
			A.Psf.Transcript.WriteErrorLine(value);
		}

		public override void WriteVerboseLine(string message)
		{
			Writing();
			Far.Net.Write("VERBOSE: " + message + "\r\n", A.Psf.Settings.VerboseForegroundColor);
			A.Psf.Transcript.WriteVerboseLine(message);
		}

		public override void WriteWarningLine(string message)
		{
			Writing();
			Far.Net.Write("WARNING: " + message + "\r\n", A.Psf.Settings.WarningForegroundColor);
			A.Psf.Transcript.WriteWarningLine(message);
		}
	}

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

	sealed class TranscriptOutputWriter : TextOutputWriter
	{
		static int _fileNameCount;
		StreamWriter _writer;
		string _fileName;

		public string FileName { get { return _fileName; } }

		public void Close()
		{
			if (_writer != null)
			{
				_writer.Close();
				_writer = null;
			}
		}

		string NewFileName()
		{
			// Tried to use the Personal folder (like PS does). For some reasons
			// some files are not deleted due to UnauthorizedAccessException.
			// It might be a virus scanner or an indexing service. Enough, the
			// files are temporary, use the Temp path. It's better to have not
			// deleted files there than in Personal.
			string directory = Path.GetTempPath();
			
			if (this == A.Psf.Transcript)
			{
				// the only session transcript
				return Path.Combine(
					directory,
					Invariant.Format("PowerShell_transcript.{0:yyyyMMddHHmmss}.txt", DateTime.Now));
			}
			else
			{
				// next instant transcript
				++_fileNameCount;
				int process = Process.GetCurrentProcess().Id;
				return Path.Combine(
					directory,
					Invariant.Format("PowerShell_transcript.{0:yyyyMMddHHmmss}.{1}.{2}.txt", DateTime.Now, process, _fileNameCount));
			}
		}

		void Writing()
		{
			if (_writer == null)
			{
				_fileName = NewFileName();
				_writer = new StreamWriter(_fileName, false, Encoding.Unicode);
				_writer.AutoFlush = true;
			}
		}

		protected override void Append(string value)
		{
			Writing();
			_writer.Write(value);
		}

		protected override void AppendLine()
		{
			Writing();
			_writer.WriteLine();
		}

		protected override void AppendLine(string value)
		{
			Writing();
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

	sealed class ExternalOutputWriter : TextOutputWriter, IDisposable //????? need?
	{
		// Output file name
		string FileName;

		// Outer viewer
		Process Process;

		// The writer
		StreamWriter Writer;

		protected override void Append(string value)
		{
			Writing();
			Writer.Write(value);
		}

		protected override void AppendLine()
		{
			Writing();
			Writer.WriteLine();
		}

		protected override void AppendLine(string value)
		{
			Writing();
			Writer.WriteLine(value);
		}

		void Writing()
		{
			// new writer
			if (Writer == null)
			{
				if (FileName == null)
					FileName = Path.GetTempFileName();

				Writer = new StreamWriter(FileName, false, Encoding.Unicode);
				Writer.AutoFlush = true;
			}

			//! start after opening a writer when BOM is already there
			StartViewer();
		}

		void StartViewer()
		{
			if (Process == null || Process.HasExited)
			{
				Process = Zoo.StartExternalViewer(FileName);
				Process.EnableRaisingEvents = true;
				Process.Exited += OnExited;
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
	}
}
